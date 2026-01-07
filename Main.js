$(function(){
  $("#activity .slider").slick({
    autoplay: true,
    autoplaySpeed: 3000,
    arrows: false,          
    centerMode: true,
    slidesToShow: 3,        
    pauseOnFocus: false,    
    pauseOnHover: false,
    responsive: [
      {
        breakpoint: 768,
        settings: {          
          slidesToShow: 1
        }
      }
    ]
  });
});