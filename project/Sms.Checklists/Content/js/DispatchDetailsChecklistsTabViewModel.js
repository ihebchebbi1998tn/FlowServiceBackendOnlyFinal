namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel = function (parentViewModel) {
	window.Main.ViewModels.ViewModelBase.call(this, arguments);
	var viewModel = this;
	viewModel.currentUser = window.ko.observable(null);
	viewModel.lookups = parentViewModel.lookups;
	viewModel.dispatch = parentViewModel.dispatch;
	viewModel.serviceOrder = parentViewModel.serviceOrder;
	viewModel.fileResourcesToRemove = ko.observableArray([]);
	var joinLocalizations = {
		Selector: "DynamicForm.Localizations",
		Operation: "filter(function(x) { return x.DynamicFormElementId == null })"
	};
	window.Main.ViewModels.GenericListViewModel.call(viewModel,
		"SmsChecklists_ServiceOrderChecklist",
		["ServiceOrderTime.PosNo", "DynamicFormKey", "CreateDate"],
		["ASC", "ASC", "ASC"],
		["DynamicForm", "DynamicForm.Languages", joinLocalizations, "ServiceOrderTime", "ServiceOrderTime.Installation"]);
	viewModel.infiniteScroll(true);
	viewModel.canAddChecklist = parentViewModel.dispatchIsEditable;
	viewModel.currentJobItemGroup = ko.pureComputed(() => window.Helper.Dispatch.getCurrentJobItemGroup(viewModel));
};
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype = Object.create(window.Main.ViewModels.GenericListViewModel.prototype);
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.init = function () {
	var viewModel = this;
	return window.Helper.User.getCurrentUser().then(function (user) {
		viewModel.currentUser(user);
		return window.Main.ViewModels.GenericListViewModel.prototype.init.apply(viewModel, arguments);
	});
};
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.checklistIsDeletable = function (serviceOrderChecklist) {
	var viewModel = this;
	return viewModel.checklistIsEditable(serviceOrderChecklist);
};
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.checklistIsEditable = function (serviceOrderChecklist) {
	var viewModel = this;
	if (serviceOrderChecklist.Completed()) {
		return false;
	}
	return viewModel.parentViewModel.dispatchIsEditable() || (viewModel.parentViewModel.dispatchIsCompletable() && !serviceOrderChecklist.SendToCustomer());
};
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.deleteServiceOrderChecklist = function (serviceOrderChecklist) {
	var viewModel = this;
	window.Helper.Confirm.confirmDelete().done(function () {
		viewModel.loading(true);
		var fileResourceIds = [];
		return window.database.CrmDynamicForms_DynamicFormResponse
			.filter("it.DynamicFormReferenceKey === this.id", { id: serviceOrderChecklist.Id })
			.toArray(function (responses) {
				responses.forEach(function (response) {
					if (response.DynamicFormElementType === "FileAttachmentDynamicFormElement" && response.Value.length) {
						fileResourceIds = fileResourceIds.concat(JSON.parse(response.Value));
					}
					window.database.remove(response);
				});
			})
			.then(function () {
				window.database.Main_FileResource.filter(function (file) {
					return file.Id in this.fileResourceIds;
				}, { fileResourceIds: fileResourceIds }).toArray(function (fileResources) {
					fileResources.forEach(function (fileResource) {
						window.database.remove(fileResource);
					});
				}).then(function () {
					window.database.remove(serviceOrderChecklist);
					return window.database.saveChanges();
				}).then(function () {
					return viewModel.filter();
				});
			});
	});
};
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.applyFilter = function (query, filterValue, filterName) {
	if (filterName === "DynamicFormTitle") {
		return query.filter("filterByDynamicFormTitle", { filter: filterValue.Value, languageKey: this.currentUser().DefaultLanguageKey, statusKey: 'Released' });
	}
	return window.Main.ViewModels.GenericListViewModel.prototype.applyFilter.apply(this, arguments);
};
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.applyOrderBy = function (query) {
	var viewModel = this;
	var id = null;
	if (viewModel.dispatch() && viewModel.dispatch().CurrentServiceOrderTimeId()) {
		id = viewModel.dispatch().CurrentServiceOrderTimeId();
	}
	query = query.orderByDescending("orderByCurrentServiceOrderTime", { currentServiceOrderTimeId: id });
	return window.Main.ViewModels.GenericListViewModel.prototype.applyOrderBy.call(viewModel, query);
};
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.applyFilters = function (query) {
	var viewModel = this;
	query = window.Main.ViewModels.GenericListViewModel.prototype.applyFilters.call(viewModel, query);
	var dispatchId = null;
	if (viewModel.dispatch()) {
		dispatchId = viewModel.dispatch().Id();
	}
	query = query
		.filter("it.ReferenceKey === this.orderId && (it.DispatchId === null || it.DispatchId === this.dispatchId)", { orderId: viewModel.serviceOrder().Id(), dispatchId: dispatchId });
	if (!ko.unwrap(viewModel.getFilter("DynamicFormTitle"))) {
		query = viewModel.applyFilter(query, "", "DynamicFormTitle");
	}
	return query;
};
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.getChecklistTitle =
	function (serviceOrderChecklist) {
		var viewModel = this;
		var dynamicForm = serviceOrderChecklist.DynamicForm();
		var currentUserDefaultLanguage = viewModel.currentUser().DefaultLanguageKey;
		var currentUserDefaultLanguageReleased = dynamicForm.Languages().some(function (x) {
			return x.LanguageKey() === currentUserDefaultLanguage && x.StatusKey() === "Released";
		});
		var localization = null;
		if (currentUserDefaultLanguageReleased) {
			localization = window.ko.utils.arrayFirst(dynamicForm.Localizations(), function (x) {
				return x.Language() === currentUserDefaultLanguage;
			});
		}
		if (!localization) {
			localization = window.ko.utils.arrayFirst(dynamicForm.Localizations(), function (x) {
				return x.Language() === dynamicForm.DefaultLanguageKey();
			});
		}
		if (localization) {
			return localization.Value();
		}
		return null;
	};
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.getItemGroup = window.Crm.Service.ViewModels.DispatchDetailsViewModel.prototype.getServicOrderPositionItemGroup;
namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.initItems = function (items) {
	var viewModel = this;
	items.sort(function (a, b) {
		return viewModel.getChecklistTitle(a).localeCompare(viewModel.getChecklistTitle(b));
	});
	if (window.Crm.Service.Settings.ServiceContract.MaintenanceOrderGenerationMode === "JobPerInstallation") {
		items.sort(function (a, b) {
			return a.itemGroup.title.localeCompare(b.itemGroup.title);
		});
	}
	return window.Main.ViewModels.GenericListViewModel.prototype.initItems.apply(this, arguments);
};


namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.deletePdfServiceOrderChecklist = async function (serviceOrderChecklist) {
	var viewModel = this;
	await window.Helper.Confirm.confirmDelete()
	viewModel.loading(true);
	var dynamicFormFileResponses = await window.database.CrmDynamicForms_DynamicFormFileResponse
		.filter("it.DynamicFormReferenceKey === this.id", { id: serviceOrderChecklist.Id() })
		.toArray()

	viewModel.fileResourcesToRemove.removeAll();
	dynamicFormFileResponses.forEach(function (response) {
		viewModel.fileResourcesToRemove().unshift(response.FileResourceId);
		window.database.remove(response);
	});

	await viewModel.fileResourcesToRemove().map((fileResourceId) => {
		return viewModel.removeFileResource(fileResourceId)
	})

	window.database.remove(serviceOrderChecklist);
	await window.database.saveChanges();

	viewModel.loading(false);
	return viewModel.filter();

};

namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.removeFileResource = function (id) {
	var deferred = new $.Deferred();
	window.database.Main_FileResource
		.find(id)
		.then(function (fileResource) {
			window.database.remove(fileResource)
			return deferred.resolve()
		})
		.fail(function (error) {
			return deferred.reject();
		})
	return deferred.promise();
};

namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.isPdf = function (serviceOrderChecklist) {
	if (serviceOrderChecklist.DynamicForm().CategoryKey() === "PDF-Checklist") {
		return true;
	}

	return false;
};

namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.pdfChecklistCounter = function (items) {
	return items
		.filter(function (r) {
			return r.DynamicForm().CategoryKey() === "PDF-Checklist";
		})
		.length;
};

namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.checklistCounter = function (items) {
	return items
		.filter(function (r) {
			return r.DynamicForm().CategoryKey() === "Checklist";
		})
		.length;
};

namespace("Crm.Service.ViewModels").DispatchDetailsChecklistsTabViewModel.prototype.filteredByPdfChecklist = function (items) {
	return items()
		.filter(function (r) {
			return r.DynamicForm().CategoryKey() === "PDF-Checklist";
		})
};