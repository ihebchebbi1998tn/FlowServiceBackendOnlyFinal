(function () {
	// Check if base exists
	if (!window.Crm || !window.Crm.Service || !window.Crm.Service.ViewModels || !window.Crm.Service.ViewModels.ServiceOrderDetailsJobsTabViewModel) {
		console.error("ServiceOrderDetailsJobsTabViewModel base not found - extension cannot load");
		return;
	}

	var baseViewModel = window.Crm.Service.ViewModels.ServiceOrderDetailsJobsTabViewModel;
	var basePrototype = baseViewModel.prototype;

	window.Crm.Service.ViewModels.ServiceOrderDetailsJobsTabViewModel = function (parentViewModel) {
		var viewModel = this;
		viewModel.lookups = parentViewModel.lookups || {};
		viewModel.lookups.serviceCaseStatuses = viewModel.lookups.serviceCaseStatuses ||
			{ $tableName: "CrmService_ServiceCaseStatus" };
		viewModel.lookups.serviceOrderTimeStatuses = viewModel.lookups.serviceOrderTimeStatuses ||
			{ $tableName: "CrmService_ServiceOrderTimeStatus" };
		viewModel.lookups.currencies = viewModel.lookups.currencies || { $tableName: "Main_Currency" };
		viewModel.serviceOrder = parentViewModel.serviceOrder;
		viewModel.timesCanBeAdded = window.ko.pureComputed(function () {
			return parentViewModel.serviceOrderIsEditable() &&
				window.Crm.Service.Settings.ServiceContract.MaintenanceOrderGenerationMode === "JobPerInstallation";
		});
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
	};

	window.Crm.Service.ViewModels.ServiceOrderDetailsJobsTabViewModel.prototype = basePrototype;
	console.log("ServiceOrderDetailsJobsTabViewModelExtension loaded successfully");
})();
