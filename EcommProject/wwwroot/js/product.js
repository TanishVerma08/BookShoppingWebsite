var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url":"/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "title", "width": "15%" },
            { "data": "description", "width": "15%" },
            { "data": "author", "width": "15%" },
            { "data": "isbn", "width": "15%" },
            { "data": "price", "width": "15%" },
            {
                "data": "isDiscontinued",
                "render": function (data, type, row) {
                    if (data) {
                        return `<button class="btn btn-success btn-sm" onclick="ToggleDiscontinue(${row.id}, true)">
                        <i class="fas fa-undo"></i> Available
                    </button>`;
                    } else {
                        return `<button class="btn btn-warning btn-sm" onclick="ToggleDiscontinue(${row.id}, false)">
                        <i class="fas fa-ban"></i> Discontinue
                    </button>`;
                    }
                },
                "width": "15%"
            },

            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="text-center">
                        <a href="/Admin/Product/Upsert/${data}" class="btn btn-info">
                            <i class="fas fa-edit"></i>
                        </a>
                        <a class="btn btn-danger" onclick=Delete('/Admin/Product/Delete/${data}')>
                            <i class="fas fa-trash-alt"></i>
                        </a>
                      
                    </div>
                    `;
                }
            }
        ]
            
    })
}
function Delete(url) {
    // alert(url);
    swal({
        title: "Want to Delete Data???",
        text: "Delete Information",
        icon: "warning",
        buttons: true,
        dangerModel: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}
function ToggleDiscontinue(id, isCurrentlyDiscontinued) {
    const actionText = isCurrentlyDiscontinued ? "make this product available" : "discontinue this product";
    const confirmText = isCurrentlyDiscontinued
        ? "This product will be marked as available again."
        : "This product will be discontinued. Pending orders will be cancelled.";

    swal({
        title: "Are you sure?",
        text: confirmText,
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willProceed) => {
        if (willProceed) {
            $.ajax({
                type: "POST",
                url: `/Admin/Product/ToggleDiscontinue/${id}`,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}
