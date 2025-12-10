(function () {
	// Check if base exists
	if (!window.Crm || !window.Crm.Service || !window.Crm.Service.ViewModels || !window.Crm.Service.ViewModels.DispatchDetailsChecklistsTabViewModel) {
		console.error("DispatchDetailsChecklistsTabViewModel base not found - extension cannot load");
		return;
	}

	var basePrototype = window.Crm.Service.ViewModels.DispatchDetailsChecklistsTabViewModel.prototype;

	// Add sortedChecklistItems as a function that returns sorted items
	// This works whether called before or after init
	// Helper function to get sorted items (current job first)
	function getSortedItems(viewModel) {
		var allItems = viewModel.items ? viewModel.items() : [];
		
		if (!allItems || allItems.length === 0) {
			return [];
		}
		
		var currentServiceOrderTimeId = null;
		if (viewModel.dispatch && viewModel.dispatch() && viewModel.dispatch().CurrentServiceOrderTimeId) {
			currentServiceOrderTimeId = viewModel.dispatch().CurrentServiceOrderTimeId();
		}
		
		if (!currentServiceOrderTimeId) {
			return allItems;
		}
		
		return allItems.slice().sort(function (a, b) {
			var aIsCurrentJob = a.ServiceOrderTimeKey && a.ServiceOrderTimeKey() === currentServiceOrderTimeId;
			var bIsCurrentJob = b.ServiceOrderTimeKey && b.ServiceOrderTimeKey() === currentServiceOrderTimeId;
			
			if (aIsCurrentJob && !bIsCurrentJob) return -1;
			if (!aIsCurrentJob && bIsCurrentJob) return 1;
			return 0;
		});
	}

	// Non-PDF checklists sorted with group tracking
	basePrototype.sortedChecklistItems = function () {
		var viewModel = this;
		var items = getSortedItems(viewModel).filter(function (item) {
			return !viewModel.isPdf(item);
		});
		
		// Mark first item of each group
		var lastServiceOrderTimeKey = null;
		items.forEach(function (item) {
			var currentKey = item.ServiceOrderTimeKey ? item.ServiceOrderTimeKey() : null;
			item._isFirstInGroup = (currentKey !== lastServiceOrderTimeKey);
			lastServiceOrderTimeKey = currentKey;
		});
		
		return items;
	};

	// PDF checklists sorted with group tracking
	basePrototype.sortedPdfChecklistItems = function () {
		var viewModel = this;
		var items = getSortedItems(viewModel).filter(function (item) {
			return viewModel.isPdf(item);
		});
		
		// Mark first item of each group
		var lastServiceOrderTimeKey = null;
		items.forEach(function (item) {
			var currentKey = item.ServiceOrderTimeKey ? item.ServiceOrderTimeKey() : null;
			item._isFirstInGroup = (currentKey !== lastServiceOrderTimeKey);
			lastServiceOrderTimeKey = currentKey;
		});
		
		return items;
	};

	console.log("DispatchDetailsChecklistsTabViewModelExtension loaded successfully");
})();
