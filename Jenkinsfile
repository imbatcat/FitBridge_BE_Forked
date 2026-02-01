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
                    dotnet build --configuration Release
                '''
            }
        }
        stage('Test') {
            steps {
                echo "Testing.."
                sh '''
                    cd FitBridge_UnitTest
                    dotnet test FitBridge_UnitTest.csproj --no-build
                '''
            }
        }
        stage('Build & Push') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'docker-credentials', 
                                          passwordVariable: 'PASS', 
                                          usernameVariable: 'USER')]) {
                    sh """
                        echo $PASS | docker login -u $USER --password-stdin
                        docker build \
                            --build-arg BUILDKIT_INLINE_CACHE=1 \
                            --cache-from ${REGISTRY}/${IMAGE_NAME}:latest \
                            -t ${REGISTRY}/${IMAGE_NAME}:${BUILD_NUMBER} \
                            -t ${REGISTRY}/${IMAGE_NAME}:latest \
                            .
                        docker push ${REGISTRY}/${IMAGE_NAME}:${BUILD_NUMBER}
                        docker push ${REGISTRY}/${IMAGE_NAME}:latest
                    """
                }
            }
        }
    }
}
