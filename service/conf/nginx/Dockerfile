FROM nginx:1.23.3

# Add arguments to set timezone
ARG TIME_ZONE
ARG LANG_NAME
ARG LANG_INPUTFILE
ARG LANG_CHARMAP

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Set timezone to Asia/Taipei
RUN ln -sf /usr/share/zoneinfo/${TIME_ZONE} /etc/localtime
# Reset tzdata software package let user set timezone take effect
RUN dpkg-reconfigure -f noninteractive tzdata

# Install base packages
RUN apt update && apt upgrade -y
RUN apt install -y locales wget gnupg2 apt-transport-https \
    ca-certificates curl software-properties-common \
    libnss3-tools iputils-ping telnet net-tools

# Set locale to specified language
# RUN localedef -i en_US -c -f UTF-8 -A /usr/share/locale/locale.alias en_US.UTF-8
RUN localedef -i ${LANG_INPUTFILE} -c -f ${LANG_CHARMAP} -A /usr/share/locale/locale.alias ${LANG_NAME}

# Clear package lists
RUN rm -rf /var/lib/apt/lists/*