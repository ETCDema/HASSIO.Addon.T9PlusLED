ARG BUILD_FROM
FROM $BUILD_FROM

# Install requirements for add-on
RUN \
  apk add \
        dotnet9-runtime

WORKDIR /app

# Copy add-on application
COPY app /app

ENTRYPOINT ["dotnet", "/app/HASSIO.Addon.T9PlusLED.dll"]