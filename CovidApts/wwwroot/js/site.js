// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function(d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) return;
    js = d.createElement(s); js.id = id;
    js.src = "https://connect.facebook.net/en_US/sdk.js#xfbml=1&version=v3.0";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));

function initMap() {
    getWeather();
    var latitudeElement = document.getElementById("Latitude");
    var longitudeElement = document.getElementById("Longitude");
    var mapElement = document.getElementById('map');

    // If there is no map element quit the init map function
    if (mapElement == null) {
        return;
    }

    // If we don't have a Latitude and Longitude elements sets the map to default location
    if (latitudeElement == null || longitudeElement == null) {
        var uluru = { lat: 31.771959, lng: 35.217018 };
        var mapZoom = 9;
    }
    else {
        var uluru = {
            lat: Number.parseFloat(latitudeElement.value),
            lng: Number.parseFloat(longitudeElement.value)
        };
        var mapZoom = 14;
    }

    // The map, centered at Uluru
    var map = new google.maps.Map(document.getElementById('map'), {
        zoom: mapZoom,
        center: uluru
    });

    // The marker, positioned at Uluru
    var marker = new google.maps.Marker({ position: uluru, map: map });
}

function addWeather(day, condition) {
   $("#weather").append('<li>' + "<div><b>" + day + "</div></b>" + "<p>" + "</p>"+'</li>');
}

function convertToCelsius(temp) {
    return Math.round(5 * (temp - 32) / 9);
}

function getWeather() {
    var latitudeElement = document.getElementById("Latitude");
    var longitudeElement = document.getElementById("Longitude");

    var DEG = "Celsius";
    var lat = 31.771959;
    var lng = 35.217018;

    //if (latitudeElement != null && longitudeElement != null) {
    //    lat = latitudeElement.value;
    //    lng = longitudeElement.value;
    //}
    
    var url = 'https://cors-anywhere.herokuapp.com/https://api.openweathermap.org/data/2.5/weather?lat=' + lat + '&lon=' + lng +'&appid=d4d7a1a00144183b60e1b8f777d932ed&units=metric'

    // Make a weather API request (it is JSONP, so CORS is not an issue):
    $.ajax({
        url: url,
        type: "GET",
        headers: { 'Access-Control-Allow-Origin': '*' },
        success: function (r) {

            console.log(r);
     

                // Create the weather items in the #scroller UL

                var item = r.main;
                addWeather("Average temperature today is:  " + item.temp + ' ° ' + DEG);          
            
        }
    });
}