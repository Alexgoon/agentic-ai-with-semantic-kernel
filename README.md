# Agentic AI with Semantic Kernel

This sample demonstrates how to build a multi-agent AI system using [Semantic Kernel](https://github.com/microsoft/semantic-kernel). The system includes three agents:

- **Planner** – creates a plan based on the user's input.
- **Reviewer** – reviews the plan and provides feedback ot the planner.
- **Executor** – carries out the plan step by step when prompted.

The solution follows a **human-in-the-loop** approach: before executing the plan, agents present it to the user for step-by-step approval.

You can run the agents using either a local model (via [Ollama](https://ollama.com)) or OpenAI's cloud models. To switch between them, update the chat completion configuration in the [`Init` method](https://github.com/Alexgoon/agentic-ai-with-semantic-kernel/blob/f1b5f8390ba2669723910c4a252319e2bd4bb406/HealthyCoding_Agentic/Infrastructure/AgentService.cs#L17):

**animation**

```csharp
// Use Ollama (local model)
builder.AddOllamaChatCompletion(modelId: "llama3.1:8b", endpoint: new Uri("http://localhost:11434/"));

// Use OpenAI (cloud model)
builder.AddOpenAIChatCompletion("gpt-4.1-mini", "[YOUR OPENAI API KEY]");
```

> [!Note]  
> This repository compliments the following YouTube video: 
