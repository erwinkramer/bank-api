param appEnvName string = 'managedEnvironment-containering-b968'
param appName string = 'app'
param revisionSuffix string = 'rev-${uniqueString(utcNow())}'
param location string = 'Germany West Central'

var appBaseUrl = 'https://${appName}.${appEnv.properties.defaultDomain}'
var sidecarResources = {
  cpu: json('0.25')
  memory: '0.5Gi'
}
var apiResources = {
  cpu: json('0.50')
  memory: '1.0Gi'
}
var apiProbeSettings = {
  type: 'Readiness'
  initialDelaySeconds: 3
  periodSeconds: 5
}
var sidecarProbeSettings = [
  {
    type: 'Startup'
    initialDelaySeconds: 0
    periodSeconds: 5
    timeoutSeconds: 2
    failureThreshold: 24
  }
  {
    type: 'Liveness'
    periodSeconds: 10
    timeoutSeconds: 2
    failureThreshold: 6
  }
  {
    type: 'Readiness'
    periodSeconds: 5
    timeoutSeconds: 2
    failureThreshold: 3
  }
]

resource appEnv 'Microsoft.App/managedEnvironments@2026-01-01' = {
  name: appEnvName
  location: location
  properties: {
    zoneRedundant: false
    workloadProfiles: [
      {
        workloadProfileType: 'Consumption'
        name: 'Consumption'
      }
    ]
    publicNetworkAccess: 'Enabled'
  }
}

resource app 'Microsoft.App/containerApps@2026-01-01' = {
  name: appName
  location: location
  kind: 'containerapps'
  identity: {
    type: 'SystemAssigned' // assign to DCR: https://learn.microsoft.com/en-us/azure/azure-monitor/containers/opentelemetry-protocol-ingestion#grant-permissions-to-the-data-collection-rule
  }
  properties: {
    managedEnvironmentId: appEnv.id
    environmentId: appEnv.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 10000
        exposedPort: 0
        transport: 'Auto'
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
        allowInsecure: false
        clientCertificateMode: 'Ignore'
        stickySessions: {
          affinity: 'none'
        }
      }
      maxInactiveRevisions: 100
    }
    template: {
      revisionSuffix: revisionSuffix
      initContainers: [] // This is what we use in kro, but we can't set probes here, for now just use regular containers
      containers: [
        {
          image: 'ghcr.io/erwinkramer/bank-api-proxy:latest'
          name: 'envoy-proxy'
          resources: sidecarResources
          probes: [
            for probe in sidecarProbeSettings: union(probe, {
              httpGet: {
                path: '/ready'
                port: 9901
                scheme: 'HTTP'
              }
            })
          ]
        }
        {
          image: 'ghcr.io/erwinkramer/bank-api:latest'
          name: 'bank-api'
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ApiDocument__Servers__0__Url'
              value: '${appBaseUrl}/v1'
            }
          ]
          resources: apiResources
          probes: [
            union(apiProbeSettings, {
              httpGet: {
                path: '/.well-known/jwks.json'
                port: 8080
                scheme: 'HTTP'
              }
            })
          ]
        }
        {
          image: 'ghcr.io/erwinkramer/bank-api-s3proxy:latest'
          name: 's3-proxy'
          resources: sidecarResources
          probes: [
            for probe in sidecarProbeSettings: union(probe, {
              httpGet: {
                path: '/healthz'
                port: 6070
                scheme: 'HTTP'
              }
            })
          ]
        }
        {
          image: 'ghcr.io/erwinkramer/bank-api-otelcol:latest'
          name: 'otel'
          resources: sidecarResources
          probes: [
            for probe in sidecarProbeSettings: union(probe, {
              httpGet: {
                path: '/health/status'
                port: 13133
                scheme: 'HTTP'
              }
            })
          ]
        }
        {
          image: 'ghcr.io/erwinkramer/bank-api-dapr:latest'
          name: 'dapr'
          resources: sidecarResources
          probes: [
            for probe in sidecarProbeSettings: union(probe, {
              httpGet: {
                path: '/v1.0/healthz'
                port: 3500
                scheme: 'HTTP'
              }
            })
          ]
        }
        {
          image: 'ghcr.io/erwinkramer/bank-api-mcp:latest'
          name: 'mcp'
          env: [
            {
              name: 'McpServerBaseUrl'
              value: '${appBaseUrl}/mcp'
            }
            {
              name: 'ApiBaseUrl'
              value: 'http://localhost:8080'
            }
            {
              name: 'ASPNETCORE_HTTP_PORTS'
              value: '8082'
            }
          ]
          resources: sidecarResources
          probes: [
            union(apiProbeSettings, {
              httpGet: {
                path: '/.well-known/oauth-protected-resource'
                port: 8082
                scheme: 'HTTP'
              }
            })
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
        cooldownPeriod: 300
        pollingInterval: 30
        rules: [
          {
            name: 'http-scaler'
            http: {
              metadata: {
                concurrentRequests: '10'
              }
            }
          }
        ]
      }
    }
  }
}
