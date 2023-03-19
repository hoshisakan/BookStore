var dataTable

$(document).ready(function () {
    initialize()
    loadDatatable()
})

function initialize() {
    $('#productSingleCreateBtn').show()
    $('#productBulkCreateBtn').show()
    $('#productSendBtn').hide()
    $('#productRevertBtn').hide()
    $('#productListFileUpload').hide()
}

function loadDatatable() {
    dataTable = $('#tblData').DataTable({
        ajax: {
            url: '/Admin/Product/GetAll',
        },
        columns: [
            { data: 'title', width: '15%' },
            { data: 'isbn', width: '15%' },
            { data: 'price', width: '15%' },
            { data: 'author', width: '15%' },
            { data: 'category.name', width: '15%' },
            {
                data: 'id',
                render: function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Product/Upsert?id=${data}"
                            class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onclick=Delete('/Admin/Product/Delete/${data}')
                            class="btn btn-danger mx-2">
                                <i class="bi bi-trash"> </i> Delete
                            </a>
                        </div>
                    `
                },
                width: '15%',
            },
        ],
    })
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!',
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload()
                        toastr.success(data.message)
                    } else {
                        toastr.error(data.message)
                    }
                },
            })
        }
    })
}

function handleBulkCreateClick() {
    $('#productSingleCreateBtn').hide()
    $('#productBulkCreateBtn').hide()
    $('#productSendBtn').show()
    $('#productRevertBtn').show()
    $('#productListFileUpload').show()
    // showMessage('onClick', 'productBulkCreateBtn')
}

function handleRevertClick() {
    $('#productSingleCreateBtn').show()
    $('#productBulkCreateBtn').show()
    $('#productSendBtn').hide()
    $('#productRevertBtn').hide()
    $('#productListFileUpload').hide()
    // showMessage('onClick', 'productRevertBtn')
}

function handleSendClick() {
    var getUploadFile = document.getElementById('productListFileUpload')

    if (getUploadFile.files.length === 0) {
        showMessage('Upload File', 'Please select a file to upload')
        return false
    }

    var form = $('#productListImportForm')[0]
    var data = new FormData(form)
    $.ajax({
        url: '/Admin/Product/BulkCreate',
        type: 'POST',
        data: data,
        contentType: false,
        processData: false,
        success: function (data) {
            if (data.success) {
                dataTable.ajax.reload()
                toastr.success(data.message)
            } else {
                toastr.error(data.message)
            }
        },
    })
}
