FROM ubuntu:22.10

# Add arguments to set timezone and dotnet version
ARG TIME_ZONE
ARG DOTNET_VERSION
ARG LANG_NAME
ARG LANG_INPUTFILE
ARG LANG_CHARMAP

# Check update and upgrade packages
RUN apt update && apt upgrade -y

# Install base packages
RUN apt install -y locales wget gnupg2 apt-transport-https \
    ca-certificates curl software-properties-common \
    libnss3-tools iputils-ping telnet net-tools \
    openssl tzdata dotnet-sdk-${DOTNET_VERSION}

# If you want to only use the runtime, uncomment the following line
# RUN apt install aspnetcore-runtime-${DOTNET_VERSION} -y

# Set locale to specified language
# RUN localedef -i en_US -c -f UTF-8 -A /usr/share/locale/locale.alias en_US.UTF-8
RUN localedef -i ${LANG_INPUTFILE} -c -f ${LANG_CHARMAP} -A /usr/share/locale/locale.alias ${LANG_NAME}

# Set timezone to Asia/Taipei
RUN ln -sf /usr/share/zoneinfo/${TIME_ZONE} /etc/localtime

# Reset tzdata software package let user set timezone take effect
RUN dpkg-reconfigure -f noninteractive tzdata

# Add the Microsoft package signing key to your list of trusted keys and add the package repository.
RUN wget https://packages.microsoft.com/config/ubuntu/22.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

# Clear package lists
RUN rm -rf /var/lib/apt/lists/*

# WORKDIR /app
WORKDIR /deploy/HoshiBookWeb

# Run the app
ENTRYPOINT [ "dotnet", "HoshiBookWeb.dll" ]