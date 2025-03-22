const { NodeSDK } = require('@opentelemetry/sdk-node');
const { getNodeAutoInstrumentations } = require('@opentelemetry/auto-instrumentations-node');
const { OTLPTraceExporter } = require('@opentelemetry/exporter-trace-otlp-grpc');
const { OTLPMetricExporter } = require('@opentelemetry/exporter-metrics-otlp-grpc');
const { OTLPLogExporter } = require("@opentelemetry/exporter-logs-otlp-grpc");
const { PeriodicExportingMetricReader } = require('@opentelemetry/sdk-metrics');
const { BatchLogRecordProcessor } = require("@opentelemetry/sdk-logs");
const { containerDetector } = require('@opentelemetry/resource-detector-container');
const { envDetector, hostDetector, osDetector, processDetector } = require('@opentelemetry/resources');

const sdk = new NodeSDK({
  instrumentations: [
    getNodeAutoInstrumentations({
      '@opentelemetry/instrumentation-fs': { enabled: false },
      '@opentelemetry/instrumentation-fastify': { enabled: false },
      '@opentelemetry/instrumentation-winston': { enabled: false },
    })
  ],
  traceExporter: new OTLPTraceExporter(),
  metricReader: new PeriodicExportingMetricReader({
    exporter: new OTLPMetricExporter(),
  }),
  logRecordProcessors: [
    new BatchLogRecordProcessor(new OTLPLogExporter())
  ],
  resourceDetectors: [
    containerDetector,
    envDetector,
    hostDetector,
    osDetector,
    processDetector,
  ],
});

sdk.start();
