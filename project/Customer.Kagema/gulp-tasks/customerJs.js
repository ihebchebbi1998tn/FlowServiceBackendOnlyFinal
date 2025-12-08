const generateJsTask = require("../../../gulp-core/gulptaskCore").generateJsTask;

module.exports = function() {
	const jsFiles = [
		"Content/js/DashboardIndexViewModelExtension.js",
		"Content/js/ServiceOrderMaterialEditModalViewModelExtension.js",
		"Content/js/ViewModels/ContactDetailsNoteTabViewModelExtension.js",
		"Content/js/ViewModels/ContactDetailsNotesTabViewModelExtension.js",
		"Content/js/ServiceOrderDetailsMaterialsTabViewModelExtension.js",
		"Content/js/ServiceOrderDetailsViewModelExtension.js",
		"Content/js/ServiceOrderDetailsJobsTabViewModelExtension.js",
		"Content/js/DispatchDetailsJobsTabViewModelExtension.js",
		"Content/js/DispatchAdHocViewModelExtension.js",
		"Content/js/DispatchDetailsViewModelExtension.js",
		"Content/js/ServiceOrderMaterialReportPlannedModalViewModel.js",		
		"Content/js/validation-rules.js",	
		"Content/js/ServiceOrderExportErrorsListIndexViewModel.js",
		"Content/js/DispatchReportPreviewModalViewModelExtension.js",
		"Content/js/DispatchDocumentAttributeEditModalViewModelExtension.js"
		];
	generateJsTask(__filename, jsFiles);
};
