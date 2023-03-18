var dataTable

$(document).ready(function () {
    loadDatatable()
})

function loadDatatable() {
    dataTable = $('#tblData').DataTable({
        ajax: {
            url: '/Admin/User/GetAllLocked',
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
            { data: 'isLockedOut', width: '15%' },
            {
                data: 'id',
                render: function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a onClick=UnLock('/Admin/User/Unlock/${data}')
                            class="btn btn-success mx-2">
                                <i class="bi bi-unlock"></i> </i> Lock
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

function Unlock(url) {
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