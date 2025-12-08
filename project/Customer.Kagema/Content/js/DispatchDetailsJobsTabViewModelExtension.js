(function () {
	// Check if base exists
	if (!window.Crm || !window.Crm.Service || !window.Crm.Service.ViewModels || !window.Crm.Service.ViewModels.DispatchDetailsJobsTabViewModel) {
		console.error("DispatchDetailsJobsTabViewModel base not found - extension cannot load");
		return;
	}

	var baseViewModel = window.Crm.Service.ViewModels.DispatchDetailsJobsTabViewModel;
	var basePrototype = baseViewModel.prototype;

	window.Crm.Service.ViewModels.DispatchDetailsJobsTabViewModel = function (parentViewModel) {
		var viewModel = this;
		viewModel.lookups = parentViewModel.lookups || {};
		viewModel.lookups.serviceCaseStatuses = viewModel.lookups.serviceCaseStatuses ||
			{ $tableName: "CrmService_ServiceCaseStatus" };
		viewModel.lookups.serviceOrderTimeStatuses = viewModel.lookups.serviceOrderTimeStatuses ||
			{ $tableName: "CrmService_ServiceOrderTimeStatus" };
		viewModel.lookups.currencies = viewModel.lookups.currencies || { $tableName: "Main_Currency" };
		viewModel.serviceOrder = parentViewModel.serviceOrder;
		
		// Include Installation and Installation.Address for Standort display
		window.Main.ViewModels.GenericListViewModel.call(viewModel, 
			"CrmService_ServiceOrderTime", 
			["PosNo"], 
			["ASC"], 
			["Installation", "Installation.Address", "Article"]);
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

	window.Crm.Service.ViewModels.DispatchDetailsJobsTabViewModel.prototype = basePrototype;
	console.log("DispatchDetailsJobsTabViewModelExtension loaded successfully");
})();
