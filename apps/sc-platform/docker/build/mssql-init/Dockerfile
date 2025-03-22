# escape=`

ARG PARENT_IMAGE
ARG HEADLESS_SERVICES_IMAGE
ARG SPE_SERVICES_IMAGE
ARG SXA_SERVICES_IMAGE

FROM ${HEADLESS_SERVICES_IMAGE} AS headless_services
FROM ${SPE_SERVICES_IMAGE} AS spe_services
FROM ${PARENT_IMAGE}

COPY --from=headless_services C:\module\db C:\resources\jss
COPY --from=spe_services C:\module\db C:\resources\spe
