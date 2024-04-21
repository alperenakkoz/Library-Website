
$(document).on("click", ".favoriteButton", function () {
    var favorites = localStorage.getItem("Favorites") || ""; //"example : 1,12,101, "
    var favoriteIds = favorites.split(',').filter(function (id) { //string array
        return id.trim() !== '';
    });
    var bookId = $(this).data("bookid");
    var index = favoriteIds.indexOf(bookId.toString());
    //console.log(index);
    if (index !== -1) {     //if exist    
        favoriteIds = favoriteIds.filter(function (value) {
            //console.log(bookId);
            return value !== bookId.toString();
        });
        //console.log(favoriteIds);
        favorites = favoriteIds.join(',') + ','; 
    } else {
        favorites += bookId + ",";
    }
    localStorage.setItem("Favorites", favorites);   
    updateButton($(this), favorites);
});

function updateButton(button, favorites) {
    var favoriteIds = favorites.split(',').filter(function (id) { //string array
        return id.trim() !== '';
    });
    var bookId = button.data("bookid");
    var isBookFavorited = favoriteIds.indexOf(bookId.toString()) === -1 ? false : true;
    if (isBookFavorited) {
        button.html('<i class="bi bi-bookmarks"></i> ' + unfavoriteText); //unfavoriteText multi language text from view
        button.removeClass("btn-primary").addClass("btn-danger");
    } else {
        button.html('<i class="bi bi-bookmarks"></i> ' + favoriteText); //favoriteText multi language text from view
        button.removeClass("btn-danger").addClass("btn-primary");
    }
}