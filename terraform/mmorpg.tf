resource "azurerm_resource_group" "k8s" {
    name     = var.resource_group_name
    location = var.location
}

resource "azurerm_kubernetes_cluster" "k8s" {
    name                       = var.cluster_name
    location                   = azurerm_resource_group.k8s.location
    resource_group_name        = azurerm_resource_group.k8s.name
    dns_prefix                 = var.dns_prefix

    linux_profile {
        admin_username = "k8sadmin"

        ssh_key {
            key_data = var.ssh_public_key
        }
    }

    default_node_pool {
        name            = "agentpool"
        node_count      = var.agent_count
        vm_size         = "Standard_D2_v2"
    }

    service_principal {
        client_id     = var.client_id
        client_secret = var.client_secret
    }

    network_profile {
        load_balancer_sku = "Standard"
        network_plugin = "kubenet"
    }

    tags = {
        Environment = "Development"
    }
}

resource "azurerm_cosmosdb_account" "db" {
  name                = "${var.cluster_name}cosmos"
  location            = azurerm_resource_group.k8s.location
  resource_group_name = azurerm_resource_group.k8s.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  capabilities {
    name = "EnableAggregationPipeline"
  }

  capabilities {
    name = "mongoEnableDocLevelTTL"
  }

  capabilities {
    name = "MongoDBv3.4"
  }

  capabilities {
      name = "EnableServerless"
  }

  consistency_policy {
    consistency_level       = "BoundedStaleness"
    max_interval_in_seconds = 10
    max_staleness_prefix    = 200
  }

  geo_location {
    location          = azurerm_resource_group.k8s.location
    failover_priority = 0
  }
}

resource "azurerm_container_registry" "acr" {
  name                     = "${var.cluster_name}acr"
  resource_group_name      = azurerm_resource_group.k8s.name
  location                 = azurerm_resource_group.k8s.location
  sku                      = "Basic"
  admin_enabled            = true
}
