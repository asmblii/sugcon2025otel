# escape=`

ARG PARENT_IMAGE
ARG SXA_SERVICES_IMAGE

FROM ${SXA_SERVICES_IMAGE} AS sxa_services

FROM ${PARENT_IMAGE}

COPY --from=sxa_services C:\module\solr\cores-sxa.json C:\data\cores-sxa.json