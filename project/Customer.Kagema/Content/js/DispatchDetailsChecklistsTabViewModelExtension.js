(function () {
	// Check if base exists
	if (!window.Crm || !window.Crm.Service || !window.Crm.Service.ViewModels || !window.Crm.Service.ViewModels.DispatchDetailsChecklistsTabViewModel) {
		console.error("DispatchDetailsChecklistsTabViewModel base not found - extension cannot load");
		return;
	}

	var basePrototype = window.Crm.Service.ViewModels.DispatchDetailsChecklistsTabViewModel.prototype;

	// Add computed observable for sorted checklist items (current job first)
	var baseInit = basePrototype.init;
	basePrototype.init = function () {
		var viewModel = this;
		
		// Call base init first
		var result = baseInit.apply(viewModel, arguments);
		
		// Create computed observable that sorts items with current job first
		viewModel.sortedChecklistItems = ko.computed(function () {
			var allItems = viewModel.items ? viewModel.items() : [];
			if (!allItems || allItems.length === 0) {
				return [];
			}
			
			// Get current ServiceOrderTime ID from dispatch
			var currentServiceOrderTimeId = null;
			if (viewModel.dispatch && viewModel.dispatch() && viewModel.dispatch().CurrentServiceOrderTimeId) {
				currentServiceOrderTimeId = viewModel.dispatch().CurrentServiceOrderTimeId();
			}
			
			if (!currentServiceOrderTimeId) {
				return allItems;
			}
			
			// Sort: current job's checklists first, then maintain original order
			return allItems.slice().sort(function (a, b) {
				var aIsCurrentJob = a.ServiceOrderTimeKey && a.ServiceOrderTimeKey() === currentServiceOrderTimeId;
				var bIsCurrentJob = b.ServiceOrderTimeKey && b.ServiceOrderTimeKey() === currentServiceOrderTimeId;
				
				if (aIsCurrentJob && !bIsCurrentJob) return -1;
				if (!aIsCurrentJob && bIsCurrentJob) return 1;
				return 0; // Keep existing order within groups
			});
		});
		
		return result;
	};

	console.log("DispatchDetailsChecklistsTabViewModelExtension loaded successfully");
})();
