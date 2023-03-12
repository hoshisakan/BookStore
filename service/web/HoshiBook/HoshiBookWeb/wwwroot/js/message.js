// Display message by modal
function showMessage(titie, message) {
    $("#info-msg-modal-title").html(titie);
    $("#info-msg-content").html(message);
    $("#info-msg-modal").modal("show");
}