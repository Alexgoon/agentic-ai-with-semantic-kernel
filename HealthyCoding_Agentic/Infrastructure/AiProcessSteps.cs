using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace HealthyCoding_Agentic.Model;


public class PlannerAgentStep : KernelProcessStep {
    public const string AgentServiceKey = $"{nameof(PlannerAgentStep)}:{nameof(AgentServiceKey)}";

    [KernelFunction]
    public async Task CreatePlan(Kernel kernel, KernelProcessStepContext context, string taskDescription) {
        var plannerAgent = kernel.Services.GetRequiredKeyedService<ChatCompletionAgent>(AgentServiceKey);

        string message = string.Format(Prompts.PlannerCreatePlanPromptTemplate, taskDescription);
        string planDescription = null;
        await foreach (var chunk in plannerAgent.InvokeStreamingAsync(message)) {
            planDescription += chunk.Message.Content;

            //Workaround to stream a plan: ideally, context.EmitEventAsync should work, but the external client doesn't receive events instantly
            WeakReferenceMessenger.Default.Send(new PlanStreamingMessage(planDescription));
        }

        await context.EmitEventAsync(StepEvents.PlanPrepared, data: new Plan(planDescription, taskDescription));
    }

    [KernelFunction]
    public async Task RefinePlan(Kernel kernel, KernelProcessStepContext context, ReviewResult reviewResult) {
        var plannerAgent = kernel.Services.GetRequiredKeyedService<ChatCompletionAgent>(AgentServiceKey);

        string userMessageText = string.Format(Prompts.PlannerRefinePlanPromptTemplate, reviewResult.Plan.TaskDescription, reviewResult.Plan.PlanDescription, reviewResult.Suggestions);
        var response = await plannerAgent.InvokeAsync(userMessageText).FirstAsync();
        string planDescription = response.Message.ToString();

        await context.EmitEventAsync(StepEvents.PlanPrepared, data: new Plan(planDescription, reviewResult.Plan.TaskDescription));
    }
}

public class ReviewerAgentStep : KernelProcessStep {
    public const string AgentServiceKey = $"{nameof(ReviewerAgentStep)}:{nameof(AgentServiceKey)}";

    [KernelFunction]
    public async Task ReviewPlanAsync(Kernel kernel, KernelProcessStepContext context, Plan plan) {
        var reviewerAgent = kernel.Services.GetRequiredKeyedService<ChatCompletionAgent>(AgentServiceKey);

        string taskDescription = char.ToLower(plan.TaskDescription[0]) + plan.TaskDescription.Substring(1);
        string message = string.Format(Prompts.ReviewerPromptTemplate, taskDescription, plan.PlanDescription);
        var response = await reviewerAgent.InvokeAsync(message).FirstAsync();
        string reviewText = response.Message.ToString();

        if (reviewText.Contains("approve", StringComparison.OrdinalIgnoreCase) && !reviewText.Contains("rejected", StringComparison.OrdinalIgnoreCase)) {
            await context.EmitEventAsync(StepEvents.PlanApproved, data: new ReviewResult(plan, reviewText, true));
        }
        else {
            await context.EmitEventAsync(StepEvents.PlanRejected, data: new ReviewResult(plan, reviewText, false));
        }
    }
}


public class ExecutorAgentStep : KernelProcessStep {
    public const string AgentServiceKey = $"{nameof(ExecutorAgentStep)}:{nameof(AgentServiceKey)}";

    [KernelFunction]
    public async Task ExecuteStep(Kernel kernel, KernelProcessStepContext context, PlannedStepFlow stepFlow) {
        var executorAgent = kernel.Services.GetRequiredKeyedService<ChatCompletionAgent>(AgentServiceKey);
        ChatHistoryAgentThread agentThread = new();
        string userMessage = string.Format(Prompts.ExecutorPromptTemplate,
            stepFlow.CurrentStep.StepDescription,
            stepFlow.ResultsHistory,
            stepFlow.Plan.PlanDescription,
            stepFlow.Plan.TaskDescription);
        agentThread.ChatHistory.AddUserMessage(userMessage);

        var response = await executorAgent.InvokeAsync(agentThread).FirstAsync();
        stepFlow.CompleteCurrentStep(response.Message.ToString());
    }
}


public class Plan(string plan, string task) {
    public Plan() : this(null, null) { }
    public string TaskDescription { get; set; } = task;
    public string PlanDescription { get; set; } = plan;
}
public class ReviewResult(Plan plan, string suggestions, bool isApproved = false) {
    public ReviewResult() : this(null, null) { }
    public Plan Plan { get; set; } = plan;
    public bool IsApproved { get; set; } = isApproved;
    public string Suggestions { get; set; } = suggestions;
}

public class PlannedStepFlow {
    public PlannedStepFlow(Plan plan) {
        Plan = plan;
        Steps = GetSplitSteps(plan.PlanDescription).Select(d => new ExecutionStep(d)).ToList();
    }
    public Plan Plan { get; set; }
    public List<ExecutionStep> Steps { get; private set; }
    public int CurrentStepIndex { get; private set; }
    public string ResultsHistory { get; private set; }
    public void CompleteCurrentStep(string stepResult) {
        CurrentStep.Result = stepResult;
        ResultsHistory += $"Step {CurrentStepIndex}. {CurrentStep.Result ?? "None"}\n";
        CurrentStepIndex++;
    }
    public ExecutionStep CurrentStep {
        get {
            if (Steps != null && CurrentStepIndex > -1 && CurrentStepIndex < Steps.Count)
                return Steps?[CurrentStepIndex];
            return null;
        }
    }
    List<string> GetSplitSteps(string planDescription) {
        var matches = Regex.Matches(planDescription, @"^\d+\.\s+(.*)", RegexOptions.Multiline);
        var items = new List<string>();
        foreach (Match match in matches) {
            items.Add(match.Groups[1].Value.Trim());
        }
        return items;
    }
}
public partial class ExecutionStep(string stepDescription) : ObservableObject {

    [ObservableProperty]
    string stepDescription = stepDescription;

    [NotifyPropertyChangedFor(nameof(Done))]
    [ObservableProperty]
    string result;
    public bool Done => Result != null;
}

public class PlanStreamingMessage(string plan) : ValueChangedMessage<string>(plan);
public class StepEvents {
    public const string StartProcess = nameof(StartProcess);
    public const string PlanPrepared = nameof(PlanPrepared);
    public const string PlanRejected = nameof(PlanRejected);
    public const string PlanApproved = nameof(PlanApproved);
    public const string ExecuteStep = nameof(ExecuteStep);
    public const string PlanPreparedExternal = nameof(PlanPreparedExternal);
    public const string PlanApprovedExternal = nameof(PlanApprovedExternal);
}

    