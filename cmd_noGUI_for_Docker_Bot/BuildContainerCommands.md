#Проверьте установку Docker:
docker --version

#Постройте Docker-образ:
docker build -t my-telegram-weather-bot .

#Запустите контейнер:
docker run -d --name my-telegram-bot-container my-telegram-weather-bot

#Проверьте, работает ли контейнер:
docker ps

#Остановка контейнера:
docker stop my-telegram-weather-bot-container

#Запуск остановленного контейнера:
docker start my-telegram-weather-bot-container

#Удаление контейнера:
docker stop my-telegram-weather-bot-container
docker rm my-telegram-weather-bot-container
