function shortenText(readMoreText, readLessText) { //parameters come from view for multi language
   
        var description = $("#Description p").html();
        var shortDescription = description.substring(0, 600);
        if (description.length > 600) {
            $("#Description p").html(shortDescription + "...");
        } else {
            $("#toggleDescription").hide();
        }

        $("#toggleDescription").click(function (e) {
            e.preventDefault();
            var linkText = $(this).text();
            if (linkText === readMoreText) {
                $("#Description p").html(description);
                $(this).text(readLessText);
            } else {
                $("#Description p").html(shortDescription + "...");
                $(this).text(readMoreText);
            }
        });
}