var dataTable

$(document).ready(function () {
    var url = window.location.search;

    initialize()

    if (url.includes("unlocked"))
    {
        loadDatatable("unlocked");
    }
    else if (url.includes("locked"))
    {
        loadDatatable("locked");
    }
    else
    {
        loadDatatable("all");
    }
})

function changeMainFeatureBtnStatus(isShow)
{
    if (isShow)
    {
        $('#singleCreateBtn').show()
        $('#bulkCreateBtn').show()
        $('#exportDetailsBtn').show()
    }
    else
    {
        $('#singleCreateBtn').hide()
        $('#bulkCreateBtn').hide()
        $('#exportDetailsBtn').hide()
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

function handleResetFileUpload() {
    $('#fileUpload').val('')
}

function initialize() {
    changeMainFeatureBtnStatus(true);
    changeFileUploadFeatureBtnStatus(false);
}

function loadDatatable(status) {
    // showMessage("status", status)
    if (status == "unlocked")
    {
        $('#filterListThead').show()
        $('#allListThead').hide()
        dataTable = $('#tblData').DataTable({
            ajax: {
                url: '/Admin/User/GetAll?status=' + status,
            },
            columns: [
                { data: 'name', width: '15%' },
                { data: 'email', width: '15%' },
                // { data: 'phoneNumber', width: '15%' },
                // { data: 'streetAddress', width: '15%' },
                // { data: 'city', width: '15%' },
                // { data: 'state', width: '15%' },
                // { data: 'postalCode', width: '15%' },
                { data: 'roleName', width: '15%' },
                { data: 'companyName', width: '15%' },
                { data: 'isLockedOut', width: '15%' },
                {
                    data: 'id',
                    render: function (data) {
                        return `
                            <div class="w-75 btn-group" role="group">
                                <a href="/Admin/User/Edit?uid=${data}"
                                class="btn btn-primary mx-2">
                                    <i class="bi bi-pencil-square"></i> Edit
                                </a>
                                <a onclick=Lock('/Admin/User/Lock/${data}')
                                class="btn btn-warning mx-2">
                                    <i class="bi bi-lock"></i> </i> Lock
                                </a>
                                <a onclick=Delete('/Admin/User/Delete/${data}')
                                class="btn btn-danger mx-2">
                                    <i class="bi bi-trash"> </i> Delete
                                </a>
                                <a href="/Admin/User/ResetPassword?uid=${data}"
                                class="btn btn-info mx-2">
                                    <i class="bi bi-key"></i> Reset
                                </a>
                            </div>
                        `
                    },
                    width: '15%',
                },
            ],
        })
    }
    else if (status == "locked")
    {
        $('#filterListThead').show()
        $('#allListThead').hide()
        dataTable = $('#tblData').DataTable({
            ajax: {
                url: '/Admin/User/GetAll?status=' + status,
            },
            columns: [
                { data: 'name', width: '20%' },
                { data: 'email', width: '20%' },
                // { data: 'phoneNumber', width: '15%' },
                // { data: 'streetAddress', width: '15%' },
                // { data: 'city', width: '15%' },
                // { data: 'state', width: '15%' },
                // { data: 'postalCode', width: '15%' },
                { data: 'roleName', width: '20%' },
                { data: 'companyName', width: '20%' },
                { data: 'isLockedOut', width: '20%' },
                {
                    data: 'id',
                    render: function (data) {
                        return `
                            <div class="w-75 btn-group" role="group">
                                <a onclick=UnLock('/Admin/User/Unlock/${data}')
                                class="btn btn-success mx-2">
                                    <i class="bi bi-unlock"></i> </i> Unlock
                                </a>
                            </div>
                        `
                    },
                    width: '15%',
                },
            ],
        })
    }
    else
    {
        $('#filterListThead').hide()
        $('#allListThead').show()
        dataTable = $('#tblData').DataTable({
            ajax: {
                url: '/Admin/User/GetAll?status=' + status,
            },
            columns: [
                { data: 'name', width: '15%' },
                { data: 'email', width: '15%' },
                { data: 'phoneNumber', width: '15%' },
                // { data: 'streetAddress', width: '15%' },
                // { data: 'city', width: '15%' },
                // { data: 'state', width: '15%' },
                // { data: 'postalCode', width: '15%' },
                { data: 'roleName', width: '15%' },
                { data: 'companyName', width: '15%' },
                { data: 'isLockedOut', width: '15%' }
            ],
        })
    }
}

function handleBulkCreateClick() {
    changeMainFeatureBtnStatus(false)
    changeFileUploadFeatureBtnStatus(true)
    handleResetFileUpload()
}

function handleRevertFileClick() {
    changeMainFeatureBtnStatus(true)
    changeFileUploadFeatureBtnStatus(false)
}

function handleResetFile() {
    handleRevertFileClick()
    handleResetFileUpload()
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure? You want to delete this user?',
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
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}

function Lock(url) {
    Swal.fire({
        title: 'Are you sure lock the user?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, locked it!',
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'POST',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}

function UnLock(url) {
    Swal.fire({
        title: 'Are you sure unlock the user?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, unlocked it!',
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'POST',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}

function handleSendFileClick() {
    var getUploadFile = document.getElementById('fileUpload')

    if (getUploadFile.files.length === 0) {
        showMessage('Upload File', 'Please select a file to upload')
        return false
    }

    var form = $('#userListImportForm')[0]
    var data = new FormData(form)
    $.ajax({
        url: '/Admin/User/BulkCreate',
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