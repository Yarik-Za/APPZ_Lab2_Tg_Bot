#Проверьте установку Docker:
docker --version

#Постройте Docker-образ:
docker build -t my-telegram-weather-bot .

#Запустите контейнер:
docker run -d --name my-telegram-bot-container my-telegram-weather-bot

#Проверьте, работает ли контейнер:
docker ps
