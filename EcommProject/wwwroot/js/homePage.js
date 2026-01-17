var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
   
        "lengthMenu": [[3, 6, 9, 12], [3, 6, 9, 12]], 
       
    });
}
