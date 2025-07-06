using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthyCoding_Agentic.Model;
using Microsoft.SemanticKernel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using HealthyCoding_Agentic.Infrastructure;

namespace HealthyCoding_Agentic.ViewModels;

public partial class MainViewModel : ObservableObject {

    [ObservableProperty]
    ObservableCollection<Customer> customers;

    [ObservableProperty]
    List<Customer> selectedCustomers = new List<Customer>();

    [ObservableProperty]
    string userTask = "Delete Emma and Jack";

    [ObservableProperty]
    string draftPlanDescription;

    [ObservableProperty]
    PlannedStepFlow planStepFlow;

    [ObservableProperty]
    bool isPlanReady;

    IAgentService UserAiAutomationService;
    IDispatcherService DispatcherService;

    public MainViewModel(IAgentService userAiAutomationService, IDispatcherService dispatcherService) {
        Customers = GetCustomers();
        UserAiAutomationService = userAiAutomationService;
        DispatcherService = dispatcherService;
        UserAiAutomationService.Init(this);

        //Workaround to stream a plan: ideally, context.EmitEventAsync should work, but the external client doesn't receive events instantly
        WeakReferenceMessenger.Default.Register<PlanStreamingMessage>(this, (s, draftPlan) => {
            DraftPlanDescription = draftPlan.Value;
        });
    }

    [RelayCommand]
    async Task SendUserTaskAsync() {
        IsPlanReady = false;
        PlanStepFlow = null;
        await UserAiAutomationService.StartNewTaskProcessAsync(UserTask, async (eventName, msg) => {
            switch (eventName) {
                case StepEvents.PlanPreparedExternal:
                    Plan plan = (Plan)msg.EventData.ToObject();
                    DraftPlanDescription = plan.PlanDescription;
                    break;
                case StepEvents.PlanApprovedExternal:
                    ReviewResult reviewResult = (ReviewResult)msg.EventData.ToObject();
                    PlanStepFlow = new PlannedStepFlow(reviewResult.Plan);
                    IsPlanReady = true;
                    break;
            }
        });
    }

    [RelayCommand(CanExecute = nameof(CanRunStep))]
    async Task RunStepAsync(ExecutionStep step) {
        await UserAiAutomationService.StartStepExecutionProcessAsync(PlanStepFlow);
    }

    bool CanRunStep(ExecutionStep step)
        => PlanStepFlow != null && PlanStepFlow.CurrentStep == step;

    #region AiReadyMethods

    [KernelFunction("add_new_customer")]
    [Description("Adds a new customer to the collection")]
    [RelayCommand]
    void AddCustomer(Customer customer) {
        if (customer == null)
            customer = new Customer();
        DispatcherService.Invoke(() => Customers.Add(customer));
    }

    [KernelFunction("batch_add_customers")]
    [Description("Adds a list of customers to the collection. Use this method when two or more customers are added.")]
    [RelayCommand]
    public void BatchAddCustomers([Description("A list of customers to add")] List<Customer> customers) {
        Application.Current.Dispatcher.Invoke(new Action(() => {
            customers.ForEach(c => Customers.Add(c));
        }));
    }

    [KernelFunction("select_customers")]
    [Description("Selects customers by their IDs")]
    public void SelectCustomers(List<int> ids) {
        SelectedCustomers = Customers.Where(c => ids.Contains(c.Id)).ToList();
    }

    [KernelFunction("get_customers")]
    [Description("Gets a list of all customers and their information: ID, Name, Email, Phone Number, Registered Date")]
    public ObservableCollection<Customer> GetCustomers()
    => Customers ??= new(Customer.GetDefaultCustomers());

    [KernelFunction("delete_selected_customers")]
    [Description("Deletes selected customers (you need to select customers before deleting them)")]
    [RelayCommand]
    void DeleteCustomers() {
        if (SelectedCustomers == null || SelectedCustomers.Count == 0)
            return;
        DispatcherService.Invoke(new Action(() => {
            foreach (var customer in SelectedCustomers.ToList()) {
                Customers.Remove(customer);
            }
        }));
    }
    #endregion AiReadyMethods
}