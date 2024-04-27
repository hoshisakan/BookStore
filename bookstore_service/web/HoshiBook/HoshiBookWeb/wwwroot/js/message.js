// Display message by modal
function showMessage(title, message) {
    $('#info-msg-head').html(title)
    $('#info-msg-content').html(message)
    $('#info-msg-modal').modal('show')
}
