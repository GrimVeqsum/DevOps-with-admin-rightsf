terraform {
  required_providers {
    yandex = {
      source = "yandex-cloud/yandex"
    }
  }
  required_version = ">= 0.12"
}

provider "yandex" {
  token     = "y0__xDhmPzWARjB3RMguoT0kBNDP55OJk8HaG6h7poiF2nJyNI6RA"
  cloud_id  = "b1gcrgvduhipovboqgc0"
  folder_id = "b1gdvic6l0c0tce4fov6"
  zone      = "ru-central1-b"
}

resource "yandex_compute_disk" "boot-disk-2" {
  name     = "boot-disk-2"
  type     = "network-ssd"
  zone     = "ru-central1-b"
  size     = "10"
  image_id = "fd8aus3bfglr6dg9hsbk"
}

data "yandex_vpc_subnet" "default_subnet" {
  name = "default-ru-central1-b"
}

resource "yandex_compute_instance" "vm-2" {
  name = "terraform3"

  resources {
    cores  = 2
    memory = 4
  }

  boot_disk {
    disk_id = yandex_compute_disk.boot-disk-2.id
  }

  network_interface {
    subnet_id = data.yandex_vpc_subnet.default_subnet.id
    nat       = true
  }

  metadata = {
    ssh-keys = "ubuntu:${file("~/.ssh/id_ed25519.pub")}"
  }

  connection {
    type        = "ssh"
    user        = "ubuntu"
    private_key = file("~/.ssh/id_ed25519")
    host        = self.network_interface.0.nat_ip_address
  }

  provisioner "remote-exec" {
  inline = [
    # Install Docker (keep as is)
    "sudo apt-get update",
    "sudo apt-get install -y apt-transport-https ca-certificates curl",
    "curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg",
    "echo \"deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable\" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null",
    "sudo apt-get update",
    "sudo apt-get install -y docker-ce docker-ce-cli containerd.io",
    "sudo usermod -aG docker ubuntu",
   
  ]
}
}

resource "yandex_vpc_network" "network-1" {
  name = "network1"
}

resource "yandex_vpc_subnet" "subnet-1" {
  name           = "subnet1"
  zone           = "ru-central1-d"
  network_id     = yandex_vpc_network.network-1.id
  v4_cidr_blocks = ["192.168.10.0/24"]
}

output "internal_ip_address_vm_2" {
  value = yandex_compute_instance.vm-2.network_interface.0.ip_address
}

output "external_ip_address_vm_2" {
  value = yandex_compute_instance.vm-2.network_interface.0.nat_ip_address
}

provider "kubernetes" {
  host                   = yandex_kubernetes_cluster.k8s-cluster.master[0].external_v4_endpoint
  cluster_ca_certificate = yandex_kubernetes_cluster.k8s-cluster.master[0].cluster_ca_certificate
  token                  = data.yandex_client_config.client.iam_token
}

data "yandex_client_config" "client" {}

resource "yandex_kubernetes_cluster" "k8s-cluster" {
  name        = "k8s-lab3"
  network_id  = yandex_vpc_network.network-1.id
  release_channel = "REGULAR"
  master {
    version = "1.29"
    public_ip = true
    zonal {
      zone      = "ru-central1-d"
      subnet_id = yandex_vpc_subnet.subnet-1.id
    }
  }
  service_account_id      = "ajeb5guordceis9joibo"
  node_service_account_id = "ajeb5guordceis9joibo"
}

resource "yandex_kubernetes_node_group" "k8s-nodes" {
  cluster_id = yandex_kubernetes_cluster.k8s-cluster.id
  name       = "k8s-nodes"
  instance_template {
    platform_id = "standard-v2"
    resources {
      cores  = 2
      memory = 4
    }
    boot_disk {
      type = "network-ssd"
      size = 64
    }
    network_interface {
      subnet_ids = [yandex_vpc_subnet.subnet-1.id]
      nat        = true
    }
  }
  scale_policy {
    fixed_scale {
      size = 2
    }
  }
}

resource "kubernetes_deployment" "dino-server" {
  metadata {
    name = "dino-server"
    labels = {
      app = "dino-server"
    }
  }
  spec {
    replicas = 1
    selector {
      match_labels = {
        app = "dino-server"
      }
    }
    template {
      metadata {
        labels = {
          app = "dino-server"
        }
      }
      spec {
        container {
          name  = "dino-server"
          image = "cr.yandex/crp10cd1h16114aqlaaj/dino-server:latest"
          env {
            name  = "DB_SERVER"
            value = "mysql"
          }
          env {
            name  = "DB_USER"
            value = "root"
          }
          env {
            name  = "DB_PASSWORD"
            value = "password"
          }
          env {
            name  = "DB_NAME"
            value = "DinoDB"
          }
          port {
            container_port = 5198
          }
          resources {
            requests = {
              cpu    = "100m"
              memory = "128Mi"
            }
            limits = {
              cpu    = "500m"
              memory = "512Mi"
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "dino-server" {
  metadata {
    name = "dino-server"
  }
  spec {
    selector = {
      app = kubernetes_deployment.dino-server.spec.0.template.0.metadata.0.labels.app
    }
    port {
      port        = 80
      target_port = 5198
    }
    type = "LoadBalancer"
  }
}

resource "kubernetes_horizontal_pod_autoscaler_v2" "dino-server-hpa" {
  metadata {
    name = "dino-server-hpa"
  }
  spec {
    scale_target_ref {
      api_version = "apps/v1"
      kind        = "Deployment"
      name        = kubernetes_deployment.dino-server.metadata[0].name
    }
    min_replicas = 1
    max_replicas = 10
    
    # Changed from metrics to metric (singular)
    metric {
      type = "Resource"
      resource {
        name = "cpu"
        target {
          type                = "Utilization"
          average_utilization = 15
        }
      }
    }
    
    behavior {
      scale_down {
        stabilization_window_seconds = 300
        select_policy               = "Min"
        policy {
          type          = "Pods"
          value         = 1
          period_seconds = 60
        }
      }
      scale_up {
        stabilization_window_seconds = 60
        select_policy               = "Max"
        policy {
          type          = "Pods"
          value         = 2
          period_seconds = 30
        }
      }
    }
  }
}

resource "kubernetes_stateful_set" "mysql" {
  metadata {
    name = "mysql"
  }

  spec {
    service_name = "mysql"
    replicas     = 1

    selector {
      match_labels = {
        app = "mysql"
      }
    }

    template {
      metadata {
        labels = {
          app = "mysql"
        }
      }

      spec {
        container {
          name  = "mysql"
          image = "cr.yandex/crp10cd1h16114aqlaaj/mysql:8.0"

          env {
            name  = "MYSQL_ROOT_PASSWORD"
            value = "password"
          }
          env {
            name  = "MYSQL_DATABASE"
            value = "DinoDB"
          }

          port {
            container_port = 3306
          }

          volume_mount {
            name       = "mysql-data"
            mount_path = "/var/lib/mysql"
          }
        }
      }
    }

    volume_claim_template {
      metadata {
        name = "mysql-data"
      }
      spec {
        access_modes = ["ReadWriteOnce"]
        resources {
          requests = {
            storage = "10Gi"
          }
        }
        storage_class_name = "yc-network-hdd"
      }
    }
  }
}

resource "kubernetes_service" "mysql" {
  metadata {
    name = "mysql"
  }
  spec {
    selector = {
      app = kubernetes_stateful_set.mysql.spec.0.template.0.metadata.0.labels.app
    }
    port {
      port        = 3306
      target_port = 3306
    }
    cluster_ip = "None"
  }
}

output "dino_server_external_ip" {
  value = kubernetes_service.dino-server.status.0.load_balancer.0.ingress.0.ip
}
