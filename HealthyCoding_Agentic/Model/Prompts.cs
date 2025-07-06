using HealthyCoding_Agentic.Model;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthyCoding_Agentic.Model
{
    public class Prompts
    {
        public const string PlannerCreatePlanPromptTemplate = 
@"

Create a plan for the user task. Reply only with a numbered list of plan steps without any introduction or summary. Avoid adding steps with only manual data processing.

## USER TASK:
{0}

## PLAN:
...";
        public const string PlannerRefinePlanPromptTemplate = 
@"

User task: Some user task
Old plan: Some plan with a missing step
Suggestions: REJECTED. You should include a step to obtain value from function F1 to get information for step N
New plan: Refined plan with a step added according to the suggestions



User task: another user task
Old plan: Some plan with an incorrect step
Suggestions: REJECTED. You should modify step K to make it more clear how parameters are transformed
New plan: Refined plan where step K is modified accoridng to the suggestions


Refine your old plan to create a new plan according to the suggestions below. Reply only with new plan without repeating old plan and suggestions.

## USER TASK:
{0}

## OLD PLAN:
{1}

## SUGGESTIONS
{2}";

        public const string ReviewerPromptTemplate = 
@"

Plan: some good plan
Response: APPROVED. This plan is built according to the rules. The sequence of function calls is logical, and the executor should be able to complete the user’s task.



Plan: some bad plan
Response: REJECTED. Step 3 uses hard-coded parameter values that are not obtained from functions in previous steps or from user input.



Plan: another good plan
Response: APPROVED. This plan meets all guidelines. Although some function parameters use hard-coded placeholders, they are clearly marked as placeholders.



Plan: another acceptable plan
Response: APPROVED. This is built according to the rules. While function parameters are hard-coded parameters, previous steps obtain data that includes required information, so the plan executor will be able to replace and, if required, format hard-coded values when following the plan.



Plan: another bad plan
Response: REJECTED. The function parameter placeholders look like real values, so the plan executor might mistakenly use them instead of real data.


Approve or reject the plan. Reply only with response without repeating the plan.

## PLAN:
To {0}, use these steps:
{1}";
        
        public const string ExecutorPromptTemplate = 
@"You must execute only the CURRENT STEP and reply with data obtained after executing instructions from the CURRENT STEP. Format data, to better serve the needs of steps in ENTIRE PLAN, but do not perform steps in ENTIRE PLAN or USER TASK. If a function in the current step doesn't return anything, write 'None' in your output.

## CURRENT STEP:
{0}

## RESULTS OR PREVIOUS STEPS:
{1}

## ENTIRE PLAN:
{2}

## USER TASK:
{3}

## CURRENT STEP OUTPUT:
...
";

        public const string PlannerInstructionsTemplate =
@"You are an attentive task planning agent. You need to generate a step-by-step plan to complete the user task.

You may use the following available functions as needed:
{0}

## RULES:
Each step in the plan should include one and only one from the list above and a short description what needs to be done. 
If a function accepts parameters not provided in user task, use other functions to obtain these parameters. If function B depends on functin A, function A should be called first.
If a step doesn't include any function calls (only manual processing), merge this step with a sibling step.
Don't include steps to confirm or verify the results.

## OUTPUT FORMAT:
1. Description: Collect information to .... Function: Function1. Parameters: None. In returned results, maually find X and Y to use them in the next step.
2. Description: Execute action to .... Function: Function2. Parameters: Process results from step 1 and pass them as parameters [param_1,param_2].";

        public const string ReviewerInstructionsTemplate =
@"You are an attentive reviewer of a plan to be executed by an AI agent.
Review the plan and ensure that the sequence of function calls will solve the user’s task.
Pay special attention to hard-coded parameters passed to functions.

Reject the plan if parameter values are assumed rather than fetched from previous steps or context.

Placeholders in function parameters (e.g., 'abc', 'id_of_Jim', 'id1') are allowed to show input format. The executor will replace them with real values later.

It is allowed to assume that data from function calls can be manually formatted before passing as parameters.

The plan can use these functions to achieve the user task:
{0}";

        public const string ExecutorInstructions =
@"Your task is to execute ONLY the CURRENT STEP of the plan.
You do not need to complete the entire plan.
However, use information from PREVIOUS RESULTS and the ENTIRE PLAN to correctly interpret and format input and output.
Your response should be concise but complete, so the next step can proceed smoothly toward completing the overall user task.

## OUTPUT FORMAT:
Respond with a single line of output that reflects the result of the CURRENT STEP.
Do NOT include phrases like ""CURRENT STEP"" or any meta-comments. Just output the result itself.";
    }
}