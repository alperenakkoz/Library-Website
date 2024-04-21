
$('a[href*="/Delete/"]').click(function (event) { //alertMessage is multi language text come from view. 
                                                  //deleteName comes from view with Model.Name 
        event.preventDefault(); 

        const confirmDelete = confirm(alertMessage + '\n' + deletedName);

        if (confirmDelete) {
            window.location.href = $(this).attr('href'); 
        }
    });
