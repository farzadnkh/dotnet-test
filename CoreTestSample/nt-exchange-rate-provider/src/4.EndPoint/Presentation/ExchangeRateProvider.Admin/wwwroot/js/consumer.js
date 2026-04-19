const modal = new bootstrap.Modal(document.getElementById("tokenModal"));
const tokenInput = $("#tokenInput");
const loaderOverlay = $("#loaderOverlay");
$(document).on("click", ".open-token-modal", function () {
	const id = $(this).data("id");

	loaderOverlay.css("display", "flex");

	$.ajax({
		url: `/Consumer/GenerateApiKey/${id}`,
		type: "GET",
		success: function (data) {
			tokenInput.val(data);
			$(".modal-dialog").removeClass("animate__fadeOutUp").addClass("animate__fadeInDown");
			loaderOverlay.hide();
			modal.show();
		},
		error: function () {
			loaderOverlay.hide();
			alert("Failed to fetch token.");
		}
	});
});

$("#tokenModal").on("hide.bs.modal", function () {
	$(".modal-dialog").removeClass("animate__fadeInDown").addClass("animate__fadeOutUp");
	setTimeout(() => {
		loaderOverlay.hide();
	}, 300);
});


$(".ok-btn").on("hide.bs.modal", function () {
	$(".modal-dialog").removeClass("animate__fadeInDown").addClass("animate__fadeOutUp");
	setTimeout(() => {
		loaderOverlay.hide();
	}, 300);
});


$("#copyTokenBtn").on("click", function () {
	navigator.clipboard.writeText(tokenInput.val()).then(() => {
		alert("Token copied to clipboard!");
	});
});



//--------------------------------------------------------------------------------------//

const deactiveModal = new bootstrap.Modal(document.getElementById("deactiveModal"));
let consumerId = 0;

$(document).on("click", ".open-deactive-modal", function () {
	consumerId = $(this).data("id");
	$(".modal-dialog").removeClass("animate__fadeOutUp").addClass("animate__fadeInDown");
	deactiveModal.show();
});

$(document).on("click", ".deactive-btn", () => {
	loaderOverlay.css("display", "flex");

	$.ajax({
		url: `/Consumer/DeActive/${consumerId}`,
		type: "GET",
		success: function () {
			loaderOverlay.hide();
			deactiveModal.hide();
			location.reload();
		},
		error: function () {
			loaderOverlay.hide();
			deactiveModal.hide();
			alert("Failed to deactivate consumer.");
		}
	});
});


$("#deacitveModal").on("hide.bs.modal", function () {
	$("#deacitveModal .modal-dialog").removeClass("animate__fadeInDown").addClass("animate__fadeOutUp");
	setTimeout(() => {
		loaderOverlay.hide();
	}, 300); // delay allows animation to finish
});

$(".cancel-btn").on("click", function () {
	$("#deacitveModal .modal-dialog").removeClass("animate__fadeInDown").addClass("animate__fadeOutUp");
	setTimeout(() => {
		deactiveModal.hide();
		loaderOverlay.hide();
	}, 300);
});