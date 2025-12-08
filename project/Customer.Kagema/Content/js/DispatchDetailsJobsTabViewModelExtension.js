(function () {
	var baseViewModel = window.Crm.Service.ViewModels.DispatchDetailsJobsTabViewModel;
	var baseConstructor = baseViewModel;

	window.Crm.Service.ViewModels.DispatchDetailsJobsTabViewModel = function (parentViewModel) {
		// Call the base ServiceOrderDetailsJobsTabViewModel first with extended expansions
		window.Main.ViewModels.ViewModelBase.apply(this, arguments);
		var viewModel = this;
		viewModel.lookups = parentViewModel.lookups;
		viewModel.lookups.serviceCaseStatuses = viewModel.lookups.serviceCaseStatuses ||
			{ $tableName: "CrmService_ServiceCaseStatus" };
		viewModel.lookups.serviceOrderTimeStatuses = viewModel.lookups.serviceOrderTimeStatuses ||
			{ $tableName: "CrmService_ServiceOrderTimeStatus" };
		viewModel.lookups.currencies = viewModel.lookups.currencies || { $tableName: "Main_Currency" };
		viewModel.serviceOrder = parentViewModel.serviceOrder;
		
		// Include Installation.Address for Standort (location) display
		window.Main.ViewModels.GenericListViewModel.call(viewModel, "CrmService_ServiceOrderTime", ["PosNo"], ["ASC"], ["Installation", "Installation.Address", "Article"]);
		viewModel.infiniteScroll(true);
		viewModel.accumulatedTotalPrice = window.ko.pureComputed(function () {
			return viewModel.items().reduce(function (partialSum, item) { return partialSum + item.totalPrice; }, 0);
		});

		// Dispatch-specific properties
		viewModel.currentServiceOrderTimeId =
			parentViewModel.dispatch && parentViewModel.dispatch() && parentViewModel.dispatch().CurrentServiceOrderTimeId()
				? parentViewModel.dispatch().CurrentServiceOrderTimeId()
				: null;
		viewModel.dispatch = parentViewModel.dispatch;
		viewModel.timesCanBeAdded = window.ko.pureComputed(function () {
			return parentViewModel.dispatchIsEditable() &&
				window.Crm.Service.Settings.ServiceContract.MaintenanceOrderGenerationMode === "JobPerInstallation";
		});
	};

	window.Crm.Service.ViewModels.DispatchDetailsJobsTabViewModel.prototype = baseViewModel.prototype;
})();
