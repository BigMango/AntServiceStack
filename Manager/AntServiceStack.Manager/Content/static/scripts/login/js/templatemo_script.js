
jQuery(function()
{
    $ = jQuery ;
    //templatemo_banner_slide camera function
    $('#templatemo_banner_slide > div').camera({
        height: 'auto',
        loader: 'bar',
        playPause: false,
        pagination: false,
        thumbnails: false,
        hover: false,
        opacityOnGrid: false,
        imagePath: 'images/',
        time: 10000
    });
    //banner slider height window height 
    //(top banner height + logo height + main menu height )
    
    changebg();
    
    //banner slider caption margin top
    //(window height - (top banner height + logo height + main menu height ) - caption height ) / 2
    banner_h1_margin_top = (($(window).height()-280) - 285)/2;
    $(".camera_caption h1").css("marginTop",banner_h1_margin_top);
    $(window).resize(function(){
        banner_h1_margin_top = (($(window).height()-280) - 285)/2;
        $(".camera_caption h1").css("marginTop",banner_h1_margin_top);
    });
  
});
    
 
function changebg(){
    banner_slider_height = $(window).outerHeight()-285;
	
	var bheight = document.documentElement.clientHeight;
	if(bheight == 0){bheight = 1000;}
    banner_slider_height = (banner_slider_height<bheight) ? bheight : banner_slider_height;
    $("#templatemo_banner_slide > div").height(banner_slider_height);
    $("#templatemo_banner_slide").height(banner_slider_height);
    $(window).resize(function(){
        banner_slider_height = $(window).outerHeight()-285;
        banner_slider_height = (banner_slider_height<bheight) ? bheight : banner_slider_height;
        $("#templatemo_banner_slide > div").height(banner_slider_height);
        $("#templatemo_banner_slide").height(banner_slider_height);
    });
	
}


window.onresize=function(){
	changebg();
};