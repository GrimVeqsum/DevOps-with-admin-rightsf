#!/bin/bash

# Конфигурация
REGISTRY_ID="crpkvrrrkm0qabqoevs4"  # Замените на ваш ID реестра
TAG="latest"                # Тег для образов

# Функция для тегирования и отправки образа
push_image() {
    local LOCAL_IMAGE=$1
    local REMOTE_IMAGE=$2
    
    echo "Тегирование $LOCAL_IMAGE -> cr.yandex/$REGISTRY_ID/$REMOTE_IMAGE:$TAG"
    docker tag "$LOCAL_IMAGE" "cr.yandex/$REGISTRY_ID/$REMOTE_IMAGE:$TAG"
    
    echo "Отправка в Yandex Registry..."
    docker push "cr.yandex/$REGISTRY_ID/$REMOTE_IMAGE:$TAG"
}

# Собираем образы из docker-compose.yml
echo "Сборка образов..."
docker-compose build

# Отправляем образы
push_image "my-csharp-server" "dino-server"  # Предполагаем, что build: . в docker-compose создаёт образ my-csharp-server
push_image "mysql:8.0" "mysql"

echo "Готово! Образы отправлены в Yandex Registry."