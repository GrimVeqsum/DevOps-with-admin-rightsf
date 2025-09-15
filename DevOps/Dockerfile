# Используем официальный образ с Android SDK
FROM ubuntu:22.04

# Устанавливаем зависимости
RUN apt-get update && apt-get install -y \
    openjdk-17-jdk \
    git \
    wget \
    unzip \
    && rm -rf /var/lib/apt/lists/*

# Устанавливаем Android SDK
ENV ANDROID_HOME /opt/android-sdk
RUN mkdir -p ${ANDROID_HOME}/cmdline-tools && \
    wget -q https://dl.google.com/android/repository/commandlinetools-linux-9477386_latest.zip -O /tmp/sdk.zip && \
    unzip -q /tmp/sdk.zip -d ${ANDROID_HOME}/cmdline-tools && \
    mv ${ANDROID_HOME}/cmdline-tools/cmdline-tools ${ANDROID_HOME}/cmdline-tools/latest && \
    rm /tmp/sdk.zip

# Добавляем Android SDK в PATH
ENV PATH ${PATH}:${ANDROID_HOME}/cmdline-tools/latest/bin:${ANDROID_HOME}/platform-tools

# Принимаем лицензии SDK
RUN yes | sdkmanager --licenses

# Устанавливаем необходимые компоненты SDK
RUN sdkmanager "platform-tools" "platforms;android-31" "build-tools;34.0.0"

# Копируем исходный код
COPY . /app
WORKDIR /app

# Собираем APK (предполагается, что проект использует Gradle)
RUN ./gradlew assembleDebug