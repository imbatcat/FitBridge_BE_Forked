pipeline {
    agent {
        node {
            label 'docker-agent-dotnet'
        }
    }
    environment {
        REGISTRY = 'rutkre'
        IMAGE_NAME = 'fitbridge-be'
    }
    stages {
        stage('Build') {
            steps {
                echo "Building.."
                sh '''
                    dotnet build --property:WarningLevel=0 --configuration Release
                '''
            }
        }
        stage('Test') {
            steps {
                echo "Testing.."
                sh '''
                    dotnet test --no-build --configuration Release
                '''
            }
        }
        stage('Build & Push') {
            steps {
                environment {
                    DOCKER_BUILDKIT = '1' // use docker buildx
                }
                script {
                    def imagePath = "rutkre/fitbridge-be"
                    def credsId = 'docker-credentials'

                    docker.withRegistry(credsId) {
                        def customImage = docker.build("${imagePath}:${env.BUILD_NUMBER}", 
                            "--build-arg BUILDKIT_INLINE_CACHE=1 " +
                            "--cache-from ${imagePath}:latest ."
                        )

                        // Push the specific build number tag
                        customImage.push()

                        // Push the 'latest' tag
                        customImage.push('latest')
                    }
                }
            }
        }
    }
}
