var dataTable;

$(document).ready(function () {
    loadDataTable();

});

function loadDataTable() {
    dataTable = $('#DT_load').DataTable({
        ajax: {
            url: "/books/getall/",
            type: "GET",
            datatype: "json"
        },
        "columns": [
            { data: "name", width: "20%" },
            { data: "author", width: "20%" },
            { data: "isbn", width: "20%" },
            {
                data: "id", render:
                    function (data) {
                        return `<div class="text-center">
                        <a href="/Books/Upsert?id=${data}" class="btn btn-success text-white" style="cursor:pointer; width:70px;">
                            Edit
                        </a>
                        &nbsp;
                        <a class="btn btn-danger text-white" style="cursor:pointer; width:70px;"
                            onclick=Delete('/Books/Delete?id='+${data})>
                            Delete
                        </a>
                        &nbsp;
                        <div class="dropdown">
                            <button onclick="toggleDropdown(${data})" class="dropbtn btn btn-info text-white">Export</button>
                            <div id="exportDropdown${data}" class="dropdown-content">
                                <a href="/Books/Export?id=${data}&format=json">JSON</a>
                                <a href="/Books/Export?id=${data}&format=xml">XML</a>
                                <a href="/Books/Export?id=${data}&format=yaml">YAML</a>
                            </div>
                        </div>`;
                    }, width: "40%"
            }
        ],
        "language": {
            "emptyTable": "No data found"
        },
        "width": "100%"
    });
}

function Delete(url) {
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}
