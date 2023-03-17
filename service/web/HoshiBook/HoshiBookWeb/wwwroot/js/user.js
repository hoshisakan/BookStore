var dataTable

$(document).ready(function () {
    loadDatatable()
})

function loadDatatable() {
    dataTable = $('#tblData').DataTable({
        ajax: {
            url: '/Admin/User/GetAll',
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
            { data: 'enabled', width: '15%' },
            {
                data: 'id',
                // render: function (data) {
                //     return `
                //         <div class="w-75 btn-group" role="group">
                //             <a href="/Admin/User/Edit?uid=${data}"
                //             class="btn btn-primary mx-2">
                //                 <i class="bi bi-pencil-square"></i> Edit
                //             </a>
                //             <a onClick=Delete('/Admin/User/Delete/${data}')
                //             class="btn btn-danger mx-2">
                //                 <i class="bi bi-trash"> </i> Delete
                //             </a>
                //             <a onClick=Disabled('/Admin/User/Disabled/${data}')
                //             class="btn btn-warning mx-2">
                //                 <i class="bi bi-trash"> </i> Disabled
                //             </a>
                //         </div>
                //     `
                // },
                render: function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/User/Edit?uid=${data}"
                            class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onClick=Delete('/Admin/User/Delete/${data}')
                            class="btn btn-danger mx-2">
                                <i class="bi bi-trash"> </i> Delete
                            </a>
                            <a href="/Admin/User/ResetPassword?uid=${data}"
                            class="btn btn-info mx-2">
                                <i class="bi bi-pencil-square"></i> Reset
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

function Disabled(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, disabled it!',
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
