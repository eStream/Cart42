$(function() {
    // toggle all checkboxes from a table when header checkbox is clicked
    $(".table th input:checkbox").click(function() {
        $checks = $(this).closest(".table").find("tbody input:checkbox");
        if ($(this).is(":checked")) {
            $checks.prop("checked", true);
        } else {
            $checks.prop("checked", false);
        }
    });
});