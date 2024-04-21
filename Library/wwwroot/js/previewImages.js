function previewImages(event) {
    var files = event.target.files;
    var imageContainer = document.getElementById("imageContainer");
    imageContainer.innerHTML = ""; 

    for (var i = 0; i < files.length; i++) {
        var reader = new FileReader();
        reader.onload = function (e) {
            var imgElement = document.createElement("img");
            imgElement.src = e.target.result;
            imgElement.width = 161;
            imgElement.height = 250;
            imageContainer.appendChild(imgElement);
        };
        reader.readAsDataURL(files[i]);
    }
}