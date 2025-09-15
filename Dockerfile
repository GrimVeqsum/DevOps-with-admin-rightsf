# Используем официальный образ Android SDK
FROM ubuntu:22.04

# Устанавливаем необходимые пакеты
RUN apt-get update && apt-get install -y \
    openjdk-17-jdk \
    wget \
    unzip \
    git \
    && rm -rf /var/lib/apt/lists/*

# Устанавливаем Android SDK
ENV ANDROID_HOME /opt/android-sdk
RUN mkdir -p ${ANDROID_HOME}/cmdline-tools && \
    wget -q https://dl.google.com/android/repository/commandlinetools-linux-9477386_latest.zip -O /tmp/cmdline-tools.zip && \
    unzip -q /tmp/cmdline-tools.zip -d ${ANDROID_HOME}/cmdline-tools && \
    mv ${ANDROID_HOME}/cmdline-tools/cmdline-tools ${ANDROID_HOME}/cmdline-tools/latest && \
    rm /tmp/cmdline-tools.zip

# Добавляем Android SDK в PATH
ENV PATH ${PATH}:${ANDROID_HOME}/cmdline-tools/latest/bin:${ANDROID_HOME}/platform-tools

# Принимаем лицензии Android SDK
RUN yes | sdkmanager --licenses

# Устанавливаем необходимые компоненты SDK
RUN sdkmanager \
    "platform-tools" \
    "build-tools;34.0.0" \
    "platforms;android-34" \
    "cmdline-tools;latest"

# Копируем исходный код в контейнер
WORKDIR /app
COPY . .

# Собираем проект (используем обертку gradlew)
RUN chmod +x ./gradlew && \
    ./gradlew assembleDebug

# Можно указать команду по умолчанию (например, запуск тестов)
CMD ["./gradlew", "test"]