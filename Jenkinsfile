pipeline {
    agent {
        node {
            label 'docker-agent-dotnet'
        }
    }
    stages {
        stage('Build') {
            steps {
                echo "Building.."
                docker --version
                sh '''
                    pwd
                    dotnet restore
                '''
            }
        }
        stage('Test') {
            steps {
                echo "Testing.."
                sh '''
                    dotnet test
                '''
            }
        }
        stage('Deliver') {
            steps {
                echo 'Deliver....'
                sh '''
                    echo "Delivered"
                '''
            }
        }
    }
}
