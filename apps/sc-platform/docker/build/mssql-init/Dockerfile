# escape=`

ARG PARENT_IMAGE
ARG HEADLESS_SERVICES_IMAGE

FROM ${HEADLESS_SERVICES_IMAGE} AS headless_services
FROM ${PARENT_IMAGE}

COPY --from=headless_services C:\module\db C:\resources\jss
