function changeTab(target) {
  let mainTabContent = document.querySelectorAll('.main-container');
  for (var i = 0; i < mainTabContent.length; i++) {
    mainTabContent[i].style.display = 'none';
  }
  mainTabContent[target].style.display = 'block'; 
}

window.changeTab = function(target) {
  let mainTabContent = document.querySelectorAll('.main-container');
  mainTabContent.forEach(tab => tab.style.display = 'none');
  if (mainTabContent[target]) {
    mainTabContent[target].style.display = 'block';
  }
}

let mainTabContent = document.querySelectorAll('.main-container');
  for (var i = 1; i < mainTabContent.length; i++) {
    mainTabContent[i].style.display = 'none';
}