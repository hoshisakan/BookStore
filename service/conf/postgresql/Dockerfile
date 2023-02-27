FROM postgres:15.2

ARG LANG_NAME
ARG LANG_INPUTFILE
ARG LANG_CHARMAP

# Set locale to specified language
# RUN localedef -i en_US -c -f UTF-8 -A /usr/share/locale/locale.alias en_US.UTF-8
RUN localedef -i ${LANG_INPUTFILE} -c -f ${LANG_CHARMAP} -A /usr/share/locale/locale.alias ${LANG_NAME}