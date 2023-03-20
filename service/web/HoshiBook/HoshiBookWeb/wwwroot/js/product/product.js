var dataTable

$(document).ready(function () {
    initialize()
    loadDatatable()
})

function changeMainFeatureBtnStatus(isShow)
{
    if (isShow)
    {
        $('#singleCreateBtn').show()
        $('#bulkCreateBtn').show()
        $('#importImageBtn').show()
    }
    else
    {
        $('#singleCreateBtn').hide()
        $('#bulkCreateBtn').hide()
        $('#importImageBtn').hide()
    }
}

function changeFileUploadFeatureBtnStatus(isShow)
{
    if (isShow)
    {
        $('#fileUpload').show()
        $('#sendFileBtn').show()
        $('#revertFileBtn').show()
    }
    else
    {
        $('#fileUpload').hide()
        $('#sendFileBtn').hide()
        $('#revertFileBtn').hide()
    }
}

function changeImageUploadFeatureBtnStatus(isShow)
{
    if (isShow)
    {
        $('#imageUpload').show()
        $('#sendImageBtn').show()
        $('#revertImageBtn').show()
    }
    else
    {
        $('#imageUpload').hide()
        $('#sendImageBtn').hide()
        $('#revertImageBtn').hide()
    }
}

function handleResetFileUpload() {
    $('#fileUpload').val('')
}

function handleResetImagesUpload() {
    $('#imageUpload').val('')
}

function initialize() {
    changeMainFeatureBtnStatus(true);
    changeFileUploadFeatureBtnStatus(false);
    changeImageUploadFeatureBtnStatus(false);
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
    changeMainFeatureBtnStatus(false)
    changeFileUploadFeatureBtnStatus(true)
    changeImageUploadFeatureBtnStatus(false)
    handleResetFileUpload();
}

function handleImportImageClick() {
    changeMainFeatureBtnStatus(false);
    changeFileUploadFeatureBtnStatus(false)
    changeImageUploadFeatureBtnStatus(true)
    handleResetImagesUpload()
}

function handleRevertFileClick() {
    changeMainFeatureBtnStatus(true)
    changeFileUploadFeatureBtnStatus(false)
}

function handleRevertImageClick() {
    changeMainFeatureBtnStatus(true)
    changeImageUploadFeatureBtnStatus(false)
}

function handleResetFile() {
    handleRevertFileClick()
    handleResetFileUpload()
}

function handleResetImage() {
    handleRevertImageClick()
    handleResetImagesUpload()
}

function handleSendFileClick() {
    var getUploadFile = document.getElementById('fileUpload')

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
                handleResetFile()
                dataTable.ajax.reload()
                toastr.success(data.message)
            } else {
                toastr.error(data.message)
            }
        },
    })
}

function handleSendImageClick() {
    var getUploadFile = document.getElementById('imageUpload')

    if (getUploadFile.files.length === 0) {
        showMessage('Upload File', 'Please select images file to upload')
        return false
    }

    var form = $('#productImageImportForm')[0]
    var data = new FormData(form)
    $.ajax({
        url: '/Admin/Product/UploadImages',
        type: 'POST',
        data: data,
        contentType: false,
        processData: false,
        success: function (data) {
            if (data.success) {
                handleResetImage()
                dataTable.ajax.reload()
                toastr.success(data.message)
            } else {
                toastr.error(data.message)
            }
        },
    })
}