(function () {
	// Check if base exists
	if (!window.Crm || !window.Crm.Service || !window.Crm.Service.ViewModels || !window.Crm.Service.ViewModels.ServiceOrderDetailsChecklistsTabViewModel) {
		console.error("ServiceOrderDetailsChecklistsTabViewModel base not found - extension cannot load");
		return;
	}

	var basePrototype = window.Crm.Service.ViewModels.ServiceOrderDetailsChecklistsTabViewModel.prototype;

	// Note: ServiceOrderDetailsChecklistsTabViewModel inherits from DispatchDetailsChecklistsTabViewModel
	// The initItems override in DispatchDetailsChecklistsTabViewModelExtension will apply here too
	// But we need to ensure the dispatch reference is accessible for ServiceOrder context

	// Store original init if we need to enhance it
	var baseInit = basePrototype.init;

	basePrototype.init = function () {
		var viewModel = this;
		
		// Ensure dispatch is accessible (inherited from parent through DispatchDetailsChecklistsTabViewModel)
		// The parentViewModel should have dispatch if we're in a dispatch context
		if (!viewModel.dispatch && viewModel.parentViewModel && viewModel.parentViewModel.dispatch) {
			viewModel.dispatch = viewModel.parentViewModel.dispatch;
		}

		return baseInit.apply(viewModel, arguments);
	};

	console.log("ServiceOrderDetailsChecklistsTabViewModelExtension loaded successfully");
})();
