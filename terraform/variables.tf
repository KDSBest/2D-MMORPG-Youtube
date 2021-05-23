variable "client_id" {}
variable "client_secret" {}

variable "agent_count" {
    default = 1
}

variable "ssh_public_key" {}

variable "dns_prefix" {
    default = "@@{GameName}@@"
}

variable cluster_name {
    default = "@@{GameName}@@"
}

variable resource_group_name {
    default = "@@{GameName}@@-rg"
}

variable location {
    default = "West Europe"
}
