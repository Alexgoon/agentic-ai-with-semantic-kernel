using HealthyCoding_Agentic.ViewModels;
using System.Globalization;
using System;
using System.Windows;
using System.Windows.Data;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Agents;
using CommunityToolkit.Mvvm.Input;
using HealthyCoding_Agentic.Model;
using HealthyCoding_Agentic.Infrastructure;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace HealthyCoding_Agentic;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    public MainWindow(MainViewModel viewModel) {
        InitializeComponent();
        DataContext = viewModel;
    }
}
//public class PlannerAgentStep2 : KernelProcessStep {
//    public const string AgentServiceKey = $"{nameof(PlannerAgentStep)}:{nameof(AgentServiceKey)}";

//    [KernelFunction]
//    public async Task CreatePlan(Kernel kernel, KernelProcessStepContext context, string taskDescription) {
//        //Get the Planner agent
//        var plannerAgent = kernel.Services.GetRequiredKeyedService<ChatCompletionAgent>(AgentServiceKey);

//        //Retrieve a response
    //    string message = string.Format(Prompts.PlannerCreatePlanPromptTemplate, taskDescription);
    //string planDescription = null;
    //    await foreach (var chunk in plannerAgent.InvokeStreamingAsync(message)) {
    //        planDescription += chunk.Message.Content;
    //    }

//        //Send an event that a plan is prepared
//        await context.EmitEventAsync(StepEvents.PlanPrepared, data: new Plan(planDescription, taskDescription));
//    }

    //[KernelFunction]
    //public async Task RefinePlan(Kernel kernel, KernelProcessStepContext context, ReviewResult reviewResult) {
    //    var plannerAgent = kernel.Services.GetRequiredKeyedService<ChatCompletionAgent>(AgentServiceKey);

    //    ChatHistoryAgentThread agentThread = new();
    //    string userMessageText = string.Format(Prompts.PlannerRefinePlanPromptTemplate, reviewResult.Plan.TaskDescription, reviewResult.Plan.PlanDescription, reviewResult.Suggestions);
    //    agentThread.ChatHistory.AddUserMessage(userMessageText);

    //    var response = await plannerAgent.InvokeAsync(agentThread).FirstAsync();
    //    string planDescription = response.Message.ToString();
    //    await context.EmitEventAsync(StepEvents.PlanPrepared, data: new Plan(planDescription, reviewResult.Plan.TaskDescription));
    //}
//}

//public class ReviewerAgentStep : KernelProcessStep {
//    public const string AgentServiceKey = $"{nameof(ReviewerAgentStep)}:{nameof(AgentServiceKey)}";

//    [KernelFunction]
//    public async Task ReviewPlanAsync(Kernel kernel, KernelProcessStepContext context, Plan plan) {
//        //Get the Reviewer agent
//        var reviewerAgent = kernel.Services.GetRequiredKeyedService<ChatCompletionAgent>(AgentServiceKey);

//        //Retrieve a response
//        string taskDescription = char.ToLower(plan.TaskDescription[0]) + plan.TaskDescription.Substring(1);
//        string message = string.Format(Prompts.ReviewerPromptTemplate, taskDescription, plan.PlanDescription);
//        var response = await reviewerAgent.InvokeAsync(message).FirstAsync();
//        string reviewText = response.Message.ToString();

//        //Send an event based on the reponse
//        if (reviewText.Contains("approve", StringComparison.OrdinalIgnoreCase) && !reviewText.Contains("rejected", StringComparison.OrdinalIgnoreCase)) {
//            await context.EmitEventAsync(StepEvents.PlanApproved, data: new ReviewResult(plan, reviewText, true));
//        }
//        else {
//            await context.EmitEventAsync(StepEvents.PlanRejected, data: new ReviewResult(plan, reviewText, false));
//        }
//    }
//}

 //KernelProcess process;
 //   void InitProcess() {
 //       ProcessBuilder processBuilder = new("Planning");
 //       var plannerStep = processBuilder.AddStepFromType<PlannerAgentStep>();
 //       var reviewerStep = processBuilder.AddStepFromType<ReviewerAgentStep>();
 //       var executorStep = processBuilder.AddStepFromType<ExecutorAgentStep>();

 //       processBuilder
 //           .OnInputEvent(StepEvents.StartProcess)
 //           .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, functionName: nameof(PlannerAgentStep.CreatePlan), parameterName: "taskDescription"));

 //       plannerStep
 //           .OnEvent(StepEvents.PlanPrepared)
 //           .SendEventTo(new ProcessFunctionTargetBuilder(reviewerStep, parameterName: "plan"));

 //       reviewerStep
 //           .OnEvent(StepEvents.PlanRejected)
 //           .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, functionName: nameof(PlannerAgentStep.RefinePlan), parameterName: "reviewResult"));

 //       process = processBuilder.Build();
 //   }




//public class ExecutorAgentStep : KernelProcessStep {
//    public const string AgentServiceKey = $"{nameof(ExecutorAgentStep)}:{nameof(AgentServiceKey)}";

//    [KernelFunction]
//    public async Task ExecuteStep(Kernel kernel, KernelProcessStepContext context, PlannedStepFlow stepFlow) {
//        var executorAgent = kernel.Services.GetRequiredKeyedService<ChatCompletionAgent>(AgentServiceKey);
//        ChatHistoryAgentThread agentThread = new();
//        string userMessage = string.Format(Prompts.ExecutorPromptTemplate,
//            stepFlow.CurrentStep.StepDescription,
//            stepFlow.ResultsHistory,
//            stepFlow.Plan.PlanDescription,
//            stepFlow.Plan.TaskDescription);
//        agentThread.ChatHistory.AddUserMessage(userMessage);

//        var response = await executorAgent.InvokeAsync(agentThread).FirstAsync();
//        stepFlow.CompleteCurrentStep(response.Message.ToString());
//    }
//}
