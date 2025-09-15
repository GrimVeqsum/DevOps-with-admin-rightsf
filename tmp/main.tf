terraform {
  required_providers {
    yandex = {
      source  = "registry.terraform.io/yandex-cloud/yandex"
      version = ">= 0.84.0"
    }
  }
}

provider "yandex" {
  service_account_key_file = "key.json"
  folder_id = "b1gdvic6l0c0tce4fov6"
  zone      = "ru-central1-a"
  endpoint = "api.cloud.yandex.net:443"
}

resource "yandex_compute_instance" "docker-vm" {
  name        = "docker-host"
  platform_id = "standard-v2"
  
  resources {
    cores  = 2
    memory = 2
  }

  boot_disk {
    initialize_params {
      image_id = "fd8vmcue7aajpmeo39kk" # Ubuntu 20.04 LTS
      size     = 20
    }
  }

  network_interface {
    subnet_id = yandex_vpc_subnet.docker-subnet.id
    nat       = true
  }

  metadata = {
    ssh-keys = "ubuntu:${file("~/.ssh/id_rsa.pub")}"
  }

  provisioner "remote-exec" {
    inline = [
      "sudo apt-get update",
      "sudo apt-get install -y apt-transport-https ca-certificates curl software-properties-common",
      "curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -",
      "sudo add-apt-repository \"deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable\"",
      "sudo apt-get update",
      "sudo apt-get install -y docker-ce",
      "sudo usermod -aG docker ubuntu"
    ]

    connection {
      type        = "ssh"
      user        = "ubuntu"
      private_key = file("~/.ssh/id_rsa")
      host        = self.network_interface[0].nat_ip_address
    }
  }
}

resource "yandex_vpc_network" "docker-network" {
  name = "docker-network"
}

resource "yandex_vpc_subnet" "docker-subnet" {
  name           = "docker-subnet"
  zone           = "ru-central1-a"
  network_id     = yandex_vpc_network.docker-network.id
  v4_cidr_blocks = ["192.168.10.0/24"]
}

output "public_ip" {
  value = yandex_compute_instance.docker-vm.network_interface[0].nat_ip_address
}