using HealthyCoding_Agentic.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HealthyCoding_Agentic.Infrastructure;

public class AgentService : IAgentService {
    string plannerInstructionsFinal;
    string reviewerInstructionsFinal;
    Kernel kernel;
    public void Init(object pluginsSourceObject) {
        var builder = Kernel.CreateBuilder();
        //Ollama
        builder.AddOllamaChatCompletion(modelId: "llama3.1:8b", endpoint: new Uri("http://localhost:11434/"));

        //OpenAI
        //builder.AddOpenAIChatCompletion("gpt-4.1-mini", "[YOUR OPEN AI API KEY]"); //For more information refer to https://platform.openai.com/api-keys

        KernelPlugin customersPlugin = KernelPluginFactory.CreateFromObject(pluginsSourceObject);
        PrepareInstructions(customersPlugin);

        builder.Services.AddKeyedSingleton(PlannerAgentStep.AgentServiceKey,
            CreateAgent(
                name: "Planner",
                instructions: plannerInstructionsFinal,
                kernel: builder.Build()));
        builder.Services.AddKeyedSingleton(ReviewerAgentStep.AgentServiceKey,
            CreateAgent(
                name: "Reviewer",
                instructions: reviewerInstructionsFinal,
                kernel: builder.Build()));
        builder.Services.AddKeyedSingleton(ExecutorAgentStep.AgentServiceKey,
            CreateAgent(
                name: "Executor",
                instructions: Prompts.ExecutorInstructions,
                kernel: builder.Build(),
                plugins: [customersPlugin],
                promptSettings: new PromptExecutionSettings() {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
                }));
        kernel = builder.Build();
        InitProcess();
    }
    ChatCompletionAgent CreateAgent(string name, string instructions, Kernel kernel, IEnumerable<KernelPlugin> plugins = null, PromptExecutionSettings promptSettings = null) {
        if (plugins != null) {
            kernel.Plugins.AddRange(plugins);
            var test = kernel.Clone();
        }
        return new() {
            Name = name,
            Instructions = instructions,
            Kernel = kernel,
            Arguments = new KernelArguments(promptSettings)
        };
    }
    void PrepareInstructions(KernelPlugin plugin) {
        string toolDescriptions = string.Empty;
        int functionNumber = 0;

        foreach (var functionMetadata in plugin.GetFunctionsMetadata()) {
            toolDescriptions += $"{++functionNumber}. Function Name: {functionMetadata.PluginName}_{functionMetadata.Name}. Description: {functionMetadata.Description}.";
            if (functionMetadata.Parameters.Count > 0)
                toolDescriptions += $"Function parameters: {string.Join(" ; ", functionMetadata.Parameters.Select(p => $"Name: {p.Name}, Description: {p.Description}, Type: {p.ParameterType}"))}.";
            toolDescriptions += "\n";
        }
        plannerInstructionsFinal = string.Format(Prompts.PlannerInstructionsTemplate, toolDescriptions);
        reviewerInstructionsFinal = string.Format(Prompts.ReviewerInstructionsTemplate, toolDescriptions);
    }

    KernelProcess process;
    void InitProcess() {
        ProcessBuilder processBuilder = new("Planning");
        var plannerStep = processBuilder.AddStepFromType<PlannerAgentStep>();
        var reviewerStep = processBuilder.AddStepFromType<ReviewerAgentStep>();
        var executorStep = processBuilder.AddStepFromType<ExecutorAgentStep>();
        var userProxyStep = processBuilder.AddProxyStep("UserProxy", [
            StepEvents.PlanPreparedExternal,
            StepEvents.PlanApprovedExternal]);

        processBuilder
            .OnInputEvent(StepEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, functionName: nameof(PlannerAgentStep.CreatePlan), parameterName: "taskDescription"));

        processBuilder
            .OnInputEvent(StepEvents.ExecuteStep)
            .SendEventTo(new ProcessFunctionTargetBuilder(executorStep, functionName: nameof(ExecutorAgentStep.ExecuteStep)));

        plannerStep
            .OnEvent(StepEvents.PlanPrepared)
            .EmitExternalEvent(userProxyStep, StepEvents.PlanPreparedExternal)
            .SendEventTo(new ProcessFunctionTargetBuilder(reviewerStep, parameterName: "plan"));

        reviewerStep
            .OnEvent(StepEvents.PlanApproved)
            .EmitExternalEvent(userProxyStep, StepEvents.PlanApprovedExternal);

        reviewerStep
            .OnEvent(StepEvents.PlanRejected)
            .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, functionName: nameof(PlannerAgentStep.RefinePlan), parameterName: "reviewResult"));

        process = processBuilder.Build();
    }

    public async Task StartNewTaskProcessAsync(string userTask, Func<string, KernelProcessProxyMessage, Task> actionCallback) {
        //We need to run the task in the background because the Ollama client operates synchronously
        //and locks the UI when streaming messages.
        await Task.Run(async () =>
            await process.StartAsync(kernel, new KernelProcessEvent {
                Id = StepEvents.StartProcess,
                Data = userTask
            },
        externalMessageChannel: new ExternalClient(actionCallback)));
    }

    public async Task StartStepExecutionProcessAsync(PlannedStepFlow stepFlow) {
        await Task.Run(async () =>
            await process.StartAsync(kernel, new KernelProcessEvent {
                Id = StepEvents.ExecuteStep,
                Data = stepFlow
            },
        externalMessageChannel: new ExternalClient(async (eventName, msg) => { })));
    }
}

public interface IAgentService {
    void Init(object pluginsSourceObject);
    Task StartNewTaskProcessAsync(string userTask, Func<string, KernelProcessProxyMessage, Task> actionCallback);
    Task StartStepExecutionProcessAsync(PlannedStepFlow stepFlow);
}

public class ExternalClient(Func<string, KernelProcessProxyMessage, Task> actionCallback) : IExternalKernelProcessMessageChannel {
    Func<string, KernelProcessProxyMessage, Task> actionCallback = actionCallback;
    public Task EmitExternalEventAsync(string externalTopicEvent, KernelProcessProxyMessage message) => actionCallback(externalTopicEvent, message);

    public ValueTask Initialize() => ValueTask.CompletedTask;

    public ValueTask Uninitialize() => ValueTask.CompletedTask;
}