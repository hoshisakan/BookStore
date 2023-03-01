# FROM ubuntu:22.10
FROM debian:11.6

# Add arguments to set timezone and dotnet version
ARG DOTNET_VERSION
ARG DOTNET_TIME_ZONE
ARG DOTNET_LANG_NAME
ARG DOTNET_LANG_INPUTFILE
ARG DOTNET_LANG_CHARMAP
ARG DEBIAN_FRONTEND
ARG DOTNET_POSTGRESQL_CLIENT_HOME

# Install base packages, build-essential libreadline-dev zlib1g-dev for make install postgresql-client 15 on debian 11
RUN apt update && apt install -y wget locales gnupg2 apt-transport-https \
    ca-certificates curl software-properties-common \
    libnss3-tools iputils-ping telnet net-tools \
    openssl tzdata build-essential libreadline-dev zlib1g-dev

# Add the Microsoft package signing key to your list of trusted keys and add the package repository.
RUN wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

RUN apt update && apt install -y dotnet-sdk-${DOTNET_VERSION}

RUN rm -rf /var/cache/apt && apt-get clean

# If you want to only use the runtime, uncomment the following line
# RUN apt install aspnetcore-runtime-${DOTNET_VERSION} -y

# Set locale to specified language
# RUN localedef -i en_US -c -f UTF-8 -A /usr/share/locale/locale.alias en_US.UTF-8
RUN localedef -i ${DOTNET_LANG_INPUTFILE} -c -f ${DOTNET_LANG_CHARMAP} -A /usr/share/locale/locale.alias ${DOTNET_LANG_NAME}

# Set timezone to Asia/Taipei
RUN ln -sf /usr/share/zoneinfo/${DOTNET_TIME_ZONE} /etc/localtime

# Reset tzdata software package let user set timezone take effect
RUN dpkg-reconfigure -f noninteractive tzdata

RUN wget https://ftp.postgresql.org/pub/source/v15.2/postgresql-15.2.tar.gz

RUN tar -zxvf postgresql-15.2.tar.gz

RUN rm postgresql-15.2.tar.gz

WORKDIR /postgresql-15.2

RUN ./configure --prefix=${DOTNET_POSTGRESQL_CLIENT_HOME}

RUN make && make install

WORKDIR /home

RUN rm -rf /postgresql-15.2

ENV PATH=$PATH:$DOTNET_POSTGRESQL_CLIENT_HOME/bin

# Clear package lists
RUN rm -rf /var/lib/apt/lists/*

# WORKDIR /app
WORKDIR /deploy/HoshiBookWeb

# Run the app
ENTRYPOINT [ "dotnet", "HoshiBookWeb.dll" ]