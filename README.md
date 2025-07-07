# Agentic AI with Semantic Kernel

> [!Note]  
> This repository compliments the following YouTube video: [Build an AI Agent that Controls Your App UI](https://youtu.be/_gpqHKWqbwA)

This sample demonstrates how to build a multi-agent AI system using [Semantic Kernel](https://github.com/microsoft/semantic-kernel). The system includes three agents:

- **Planner** – creates a plan based on the user's input.
- **Reviewer** – reviews the plan and provides feedback to the planner.
- **Executor** – carries out the plan step by step when prompted.

The solution follows a **human-in-the-loop** approach: before executing the plan, agents present it to the user for step-by-step approval.

![Demo](Images/Demo_Animation.gif)

## How to Run the Project

You can run the agents using either a local model (via [Ollama](https://ollama.com)) or OpenAI's cloud models.

### Run with Ollama

To use a local Ollama-based model, configure the Ollama chat completion in the [`Init` method`](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/f1b5f8390ba2669723910c4a252319e2bd4bb406/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L17):

```csharp
builder.AddOllamaChatCompletion(modelId: "llama3.1:8b", endpoint: new Uri("http://localhost:11434/"));
```
You’ll also need to [install Ollama](https://ollama.com/download) and run the following command in the command prompt:

> ollama run llama3.1:8b
This command will download and launch the LLaMA model locally.

### Run with OpenAI

To use OpenAI instead of a local model, configure the OpenAI chat completion in the [`Init` method](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/f1b5f8390ba2669723910c4a252319e2bd4bb406/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L17)

```csharp
builder.AddOpenAIChatCompletion("gpt-4.1-mini", "[YOUR OPENAI API KEY]");
```

You can create an API key on the [OpenAI API keys page](https://platform.openai.com/api-keys). If you weren’t granted free credits during registration, you may need to top up your balance with a minimum payment (usually around $5).

## Implementation Details

Below are the key steps we follow to build the system:

1. **Initialize the Semantic Kernel** (build the kernel and register services):
```csharp
var builder = Kernel.CreateBuilder();
builder.AddOllamaChatCompletion(modelId: "llama3.1:8b", endpoint: new Uri("http://localhost:11434/"));
builder.Services.AddKeyedSingleton(PlannerAgentStep.AgentServiceKey,
```
[(AgentService.cs: Init)](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/a245564b60d9eeb0e859c29580346b076361d257/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L18-L20)


2. **Create agents using the `ChatCompletionAgent` class**:
```csharp
ChatCompletionAgent CreateAgent(string name, string instructions, Kernel kernel, IEnumerable<KernelPlugin> plugins = null, PromptExecutionSettings promptSettings = null) {
//...
return new() { Name = name,
               Instructions = instructions,
               Kernel = kernel,
               Arguments = new KernelArguments(promptSettings)
};}
```
[(AgentService.cs: CreateAgent)](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/a245564b60d9eeb0e859c29580346b076361d257/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L50-L61)

3. **Add plugins (tools) to the Executor agent**:
```csharp
KernelPlugin customersPlugin = KernelPluginFactory.CreateFromObject(pluginsSourceObject);
//...
kernel.Plugins.AddRange(plugins);
};}
```
[(AgentService.cs: Init)](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/a245564b60d9eeb0e859c29580346b076361d257/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L52)

4. **Define process steps for each agent**:
```csharp
public class PlannerAgentStep : KernelProcessStep {
    [KernelFunction]
    public async Task CreatePlan(Kernel kernel, KernelProcessStepContext context, string taskDescription) {
        //...
    }

    [KernelFunction]
    public async Task RefinePlan(Kernel kernel, KernelProcessStepContext context, ReviewResult reviewResult) {
        //...
    }
}
public class ReviewerAgentStep : KernelProcessStep {

    [KernelFunction]
    public async Task ReviewPlanAsync(Kernel kernel, KernelProcessStepContext context, Plan plan) {
        //...
    }
}


public class ExecutorAgentStep : KernelProcessStep {

    [KernelFunction]
    public async Task ExecuteStep(Kernel kernel, KernelProcessStepContext context, PlannedStepFlow stepFlow) {
        //...
    }
}
```
[(AiProcessSteps.cs)](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/a245564b60d9eeb0e859c29580346b076361d257/HealthyCoding_Agentic/Infrastructure/AiProcessSteps.cs#L17-L87)

5. **Define the process flow** (i.e., how data is transferred between steps).  
   For example, the planner sends the plan to the reviewer, who then sends feedback back to the planner for refinement:

```csharp
  ProcessBuilder processBuilder = new("Planning");
  var plannerStep = processBuilder.AddStepFromType<PlannerAgentStep>();
  var reviewerStep = processBuilder.AddStepFromType<ReviewerAgentStep>();
  //...

  processBuilder
      .OnInputEvent(StepEvents.StartProcess)
      .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, functionName: nameof(PlannerAgentStep.CreatePlan), parameterName: "taskDescription"));
  
  plannerStep
      .OnEvent(StepEvents.PlanPrepared)
      .SendEventTo(new ProcessFunctionTargetBuilder(reviewerStep, parameterName: "plan"));
  
  reviewerStep
      .OnEvent(StepEvents.PlanRejected)
      .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, functionName: nameof(PlannerAgentStep.RefinePlan), parameterName: "reviewResult"));
  
  process = processBuilder.Build();
//...
return new() { Name = name,
               Instructions = instructions,
               Kernel = kernel,
               Arguments = new KernelArguments(promptSettings)
};}
```
[(AgentService.cs: InitProcess)](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/a245564b60d9eeb0e859c29580346b076361d257/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L77-L108)

6. **Implement human-in-the-loop support with an external client**:
  - **Add a user proxy step**
    ```csharp
        var userProxyStep = processBuilder.AddProxyStep("UserProxy", [
            StepEvents.PlanPreparedExternal,
            StepEvents.PlanApprovedExternal]);
    };
    ```
    [(AgentService.cs: InitProcess)](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/a245564b60d9eeb0e859c29580346b076361d257/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L82-L84)
    
  - **Emit an external event**:
    ```csharp
        reviewerStep
            .OnEvent(StepEvents.PlanApproved)
            .EmitExternalEvent(userProxyStep, StepEvents.PlanApprovedExternal);
    };
    ```
    [(AgentService.cs: InitProcess)](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/a245564b60d9eeb0e859c29580346b076361d257/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L99-L100)
    
  - **Create a client message channel**:
    ```csharp
        public class ExternalClient(Func<string, KernelProcessProxyMessage, Task> actionCallback)
            : IExternalKernelProcessMessageChannel {
    
          Func<string, KernelProcessProxyMessage, Task> actionCallback = actionCallback;
          public Task EmitExternalEventAsync(string externalTopicEvent, KernelProcessProxyMessage message) => actionCallback(externalTopicEvent, message);  
          public ValueTask Initialize() => ValueTask.CompletedTask;
          public ValueTask Uninitialize() => ValueTask.CompletedTask;
        }
    ```
    [(AgentService.cs: ExternalClient)](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/a245564b60d9eeb0e859c29580346b076361d257/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L137-L144)
    
  - **Pass the message channel to the process**:
    ```csharp
          await Task.Run(async () =>
            await process.StartAsync(kernel, new KernelProcessEvent {
                Id = StepEvents.StartProcess,
                Data = userTask
            },
        externalMessageChannel: new ExternalClient(actionCallback)));
    ```
    [(AgentService.cs: StartNewTaskProcessAsync)](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/a245564b60d9eeb0e859c29580346b076361d257/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L118)
    
